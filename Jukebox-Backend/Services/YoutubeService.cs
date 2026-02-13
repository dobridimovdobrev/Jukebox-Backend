using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Jukebox_Backend.Models.Dto.ExternalApiData;
using Serilog;

namespace Jukebox_Backend.Services
{
    public class YouTubeApiService
    {
        private readonly List<string> apiKeys;
        private int currentKeyIndex = 0;
        private bool allKeysExhausted = false;
        private YouTubeService youtubeService;

        public YouTubeApiService(IConfiguration configuration)
        {
            var keys = configuration.GetSection("ApiKeys:YouTube").Get<string[]>();

            if (keys == null || keys.Length == 0)
            {
                var singleKey = configuration["ApiKeys:YouTube"];
                if (string.IsNullOrEmpty(singleKey))
                    throw new Exception("YouTube API key not configured");

                keys = new[] { singleKey };
            }

            apiKeys = keys.ToList();
            youtubeService = CreateService(apiKeys[0]);

            Log.Information("YOUTUBE: Loaded {Count} API keys", apiKeys.Count);
        }

        private YouTubeService CreateService(string apiKey)
        {
            return new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "Jukebox"
            });
        }

        private bool TryNextKey()
        {
            currentKeyIndex++;

            if (currentKeyIndex >= apiKeys.Count)
            {
                currentKeyIndex = 0;
                return false;
            }

            Log.Warning("YOUTUBE: Key {Index} exhausted, rotating to key {Next}",
                currentKeyIndex - 1, currentKeyIndex);

            youtubeService = CreateService(apiKeys[currentKeyIndex]);
            return true;
        }

        public async Task<YoutubeResult?> SearchOfficialVideoAsync(string artistName, string songTitle)
        {
            var attempts = apiKeys.Count;

            while (attempts > 0)
            {
                try
                {
                    var query = $"\"{artistName}\" \"{songTitle}\" official audio";

                    var searchRequest = youtubeService.Search.List("snippet");
                    searchRequest.Q = query;
                    searchRequest.Type = "video";
                    searchRequest.VideoCategoryId = "10";
                    searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
                    searchRequest.MaxResults = 5;
                    searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;

                    var searchResponse = await searchRequest.ExecuteAsync();

                    foreach (var item in searchResponse.Items)
                    {
                        if (IsValidMusicVideo(item, artistName, songTitle))
                        {
                            return new YoutubeResult
                            {
                                VideoId = item.Id.VideoId,
                                Title = item.Snippet.Title,
                                ChannelTitle = item.Snippet.ChannelTitle,
                                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url
                            };
                        }
                    }

                    var firstResult = searchResponse.Items.FirstOrDefault();
                    if (firstResult != null)
                    {
                        return new YoutubeResult
                        {
                            VideoId = firstResult.Id.VideoId,
                            Title = firstResult.Snippet.Title,
                            ChannelTitle = firstResult.Snippet.ChannelTitle,
                            ThumbnailUrl = firstResult.Snippet.Thumbnails.Medium.Url
                        };
                    }

                    return null;
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Log.Warning("YOUTUBE: Quota exceeded on key {Index}: {Msg}", currentKeyIndex, ex.Message);

                    if (!TryNextKey())
                    {
                        allKeysExhausted = true;
                        Log.Error("YOUTUBE: All API keys exhausted");
                        return null;
                    }

                    attempts--;
                }
                catch (Exception ex)
                {
                    Log.Error("YOUTUBE ERROR: {Msg}", ex.Message);
                    return null;
                }
            }

            return null;
        }

        public bool IsQuotaExhausted() => allKeysExhausted;

        private bool IsValidMusicVideo(SearchResult video, string artistName, string songTitle)
        {
            if (video?.Snippet == null)
                return false;

            var title = video.Snippet.Title?.ToLower() ?? string.Empty;
            var channel = video.Snippet.ChannelTitle?.ToLower() ?? string.Empty;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(channel))
                return false;

            if (!title.Contains(songTitle.ToLower()))
                return false;

            if (!title.Contains(artistName.ToLower()) && !channel.Contains(artistName.ToLower()))
                return false;

            var blacklist = new[]
            {
                "cover", "covers",
                "live", "live performance", "concert",
                "remix", "remixed",
                "karaoke", "instrumental",
                "reaction", "reacts to",
                "lyrics", "lyric video",
                "tutorial", "how to play", "lesson",
                "parody", "funny",
                "acoustic version",
                "slowed", "reverb", "nightcore"
            };

            if (blacklist.Any(word => title.Contains(word)))
                return false;

            var whitelist = new[] { "official", "official audio", "official video", "vevo" };
            var hasWhitelist = whitelist.Any(word => title.Contains(word) || channel.Contains(word));

            if (!hasWhitelist && !channel.Contains(artistName.ToLower()))
                return false;

            return true;
        }
    }
}
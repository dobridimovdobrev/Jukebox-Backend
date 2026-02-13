using System.Text.Json;
using System.Text.Json.Serialization;
using Jukebox_Backend.Models.Dto.ExternalApiData;
using Serilog;

namespace Jukebox_Backend.Services
{
    public class TheAudioDBService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;
        private const string BaseUrl = "https://www.theaudiodb.com/api/v1/json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TheAudioDBService(IConfiguration configuration, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            apiKey = configuration["ApiKeys:TheAudioDB"]
                ?? throw new Exception("TheAudioDB API key not configured");
        }

        // cerca artista per nome
        public async Task<TheAudioDbArtistResult?> SearchArtistAsync(string artistName)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/search.php?s={Uri.EscapeDataString(artistName)}";
                var json = await FetchAsync(url);
                if (json == null) return null;

                var result = JsonSerializer.Deserialize<ArtistResponse>(json, JsonOptions);
                return result?.Artists?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB SEARCH ARTIST ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // lookup artista per musicbrainz id
        public async Task<TheAudioDbArtistResult?> GetArtistByMbidAsync(string mbid)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/artist-mb.php?i={mbid}";
                var json = await FetchAsync(url);
                if (json == null) return null;

                var result = JsonSerializer.Deserialize<ArtistResponse>(json, JsonOptions);
                return result?.Artists?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB ARTIST BY MBID ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // tutti gli album per audiodb artist id
        public async Task<List<TheAudioDbAlbumResult>> GetAlbumsByArtistIdAsync(string artistId)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/album.php?i={artistId}";
                var json = await FetchAsync(url);
                if (json == null) return new List<TheAudioDbAlbumResult>();

                var result = JsonSerializer.Deserialize<AlbumResponse>(json, JsonOptions);
                return result?.Album ?? new List<TheAudioDbAlbumResult>();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB ALBUMS BY ARTIST ID ERROR: {Msg}", ex.Message);
                return new List<TheAudioDbAlbumResult>();
            }
        }

        // album singolo per musicbrainz release-group id
        public async Task<List<TheAudioDbAlbumResult>> GetAlbumsByMbidAsync(string mbid)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/album-mb.php?i={mbid}";
                var json = await FetchAsync(url);
                if (json == null) return new List<TheAudioDbAlbumResult>();

                var result = JsonSerializer.Deserialize<AlbumResponse>(json, JsonOptions);
                return result?.Album ?? new List<TheAudioDbAlbumResult>();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB ALBUMS BY MBID ERROR: {Msg}", ex.Message);
                return new List<TheAudioDbAlbumResult>();
            }
        }

        // tracks per album id
        public async Task<List<TheAudioDbTrackResult>> GetTracksByAlbumIdAsync(string albumId)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/track.php?m={albumId}";
                var json = await FetchAsync(url);
                if (json == null) return new List<TheAudioDbTrackResult>();

                var result = JsonSerializer.Deserialize<TrackResponse>(json, JsonOptions);
                return result?.Track ?? new List<TheAudioDbTrackResult>();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB TRACKS BY ALBUM ERROR: {Msg}", ex.Message);
                return new List<TheAudioDbTrackResult>();
            }
        }

        // music videos per artist id
        public async Task<List<TheAudioDbTrackResult>> GetMusicVideosByArtistIdAsync(string artistId)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/mvid.php?i={artistId}";
                var json = await FetchAsync(url);
                if (json == null) return new List<TheAudioDbTrackResult>();

                var result = JsonSerializer.Deserialize<MvidResponse>(json, JsonOptions);
                return result?.Mvids ?? new List<TheAudioDbTrackResult>();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB MVIDS BY ARTIST ID ERROR: {Msg}", ex.Message);
                return new List<TheAudioDbTrackResult>();
            }
        }

        // music videos per musicbrainz id
        public async Task<List<TheAudioDbTrackResult>> GetMusicVideosByMbidAsync(string mbid)
        {
            try
            {
                var url = $"{BaseUrl}/{apiKey}/mvid-mb.php?i={mbid}";
                var json = await FetchAsync(url);
                if (json == null) return new List<TheAudioDbTrackResult>();

                var result = JsonSerializer.Deserialize<MvidResponse>(json, JsonOptions);
                return result?.Mvids ?? new List<TheAudioDbTrackResult>();
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB MVIDS BY MBID ERROR: {Msg}", ex.Message);
                return new List<TheAudioDbTrackResult>();
            }
        }

        // estrai youtube video id da url
        public static string? ExtractYoutubeId(string? musicVidUrl)
        {
            if (string.IsNullOrEmpty(musicVidUrl))
                return null;

            try
            {
                if (musicVidUrl.Contains("watch?v="))
                {
                    var id = musicVidUrl.Split("watch?v=")[1].Split('&')[0];
                    return string.IsNullOrEmpty(id) ? null : id;
                }

                if (musicVidUrl.Contains("youtu.be/"))
                {
                    var id = musicVidUrl.Split("youtu.be/")[1].Split('?')[0];
                    return string.IsNullOrEmpty(id) ? null : id;
                }
            }
            catch { }

            return null;
        }

        // parse durata da millisecondi stringa
        public static int ParseDurationSeconds(string? milliseconds)
        {
            if (string.IsNullOrEmpty(milliseconds)) return 0;
            if (int.TryParse(milliseconds, out var ms)) return ms / 1000;
            return 0;
        }

        // parse anno stringa a int
        public static int? ParseYear(string? yearString)
        {
            if (string.IsNullOrEmpty(yearString)) return null;
            if (int.TryParse(yearString, out var year) && year > 1800 && year < 2100)
                return year;
            return null;
        }

        // fetch con logging
        private async Task<string?> FetchAsync(string url)
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("AUDIODB HTTP {Status}: {Url}", response.StatusCode, url);
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        // json response 
        private class ArtistResponse
        {
            [JsonPropertyName("artists")]
            public List<TheAudioDbArtistResult>? Artists { get; set; }
        }

        private class AlbumResponse
        {
            [JsonPropertyName("album")]
            public List<TheAudioDbAlbumResult>? Album { get; set; }
        }

        private class TrackResponse
        {
            [JsonPropertyName("track")]
            public List<TheAudioDbTrackResult>? Track { get; set; }
        }

        private class MvidResponse
        {
            [JsonPropertyName("mvids")]
            public List<TheAudioDbTrackResult>? Mvids { get; set; }
        }
    }
}
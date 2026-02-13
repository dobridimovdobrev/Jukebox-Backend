using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Jukebox_Backend.Models.Dto.ExternalApiData;
using Serilog;

namespace Jukebox_Backend.Services
{
    public class MusicBrainzService
    {
        private readonly Query query;

        public MusicBrainzService()
        {
            query = new Query("Jukebox", "1.0", new Uri("mailto:dobri_dobrev@yahoo.com"));
        }

        public async Task<List<MusicBrainzArtistResult>> SearchArtistsAsync(string artistName, int limit = 10)
        {
            try
            {
                var searchResults = await query.FindArtistsAsync(artistName, limit);
                var results = new List<MusicBrainzArtistResult>();

                foreach (var artist in searchResults.Results)
                {
                    results.Add(new MusicBrainzArtistResult
                    {
                        MusicBrainzId = artist.Item.Id.ToString(),
                        Name = artist.Item.Name ?? string.Empty,
                        CountryCode = artist.Item.Area?.Iso31661Codes?.FirstOrDefault(),
                        CareerStart = ParseDate(artist.Item.LifeSpan?.Begin?.ToString()),
                        CareerEnd = ParseDate(artist.Item.LifeSpan?.End?.ToString()),
                        Score = artist.Score
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                Log.Error("MUSICBRAINZ SEARCH ERROR: {Msg}", ex.Message);
                return new List<MusicBrainzArtistResult>();
            }
        }

        public async Task<List<MusicBrainzSongResult>> GetArtistRecordingsAsync(
            Guid artistMbid,
            int? yearFrom = null,
            int? yearTo = null,
            int limit = 100)
        {
            try
            {
                Log.Information("MUSICBRAINZ: Browsing recordings for {Mbid}", artistMbid);
                var recordings = await query.BrowseArtistRecordingsAsync(artistMbid, limit: limit);
                var results = new List<MusicBrainzSongResult>();

                Log.Information("MUSICBRAINZ: Browse returned {Count} recordings", recordings.Results.Count);

                if (recordings.Results.Count == 0)
                {
                    Log.Information("MUSICBRAINZ: Falling back to search for arid:{Mbid}", artistMbid);
                    var searchResults = await query.FindRecordingsAsync($"arid:{artistMbid}", limit: limit);

                    Log.Information("MUSICBRAINZ: Search returned {Count} recordings", searchResults.Results.Count);

                    foreach (var result in searchResults.Results)
                    {
                        var recording = result.Item;
                        var releaseYear = GetReleaseYear(recording);

                        if (yearFrom.HasValue && releaseYear.HasValue && releaseYear < yearFrom)
                            continue;
                        if (yearTo.HasValue && releaseYear.HasValue && releaseYear > yearTo)
                            continue;

                        results.Add(new MusicBrainzSongResult
                        {
                            MusicBrainzId = recording.Id.ToString(),
                            Title = recording.Title ?? string.Empty,
                            Duration = GetDurationSeconds(recording.Length),
                            ReleaseYear = releaseYear ?? 0,
                            IsrcCode = recording.Isrcs?.FirstOrDefault()
                        });
                    }

                    return results
                        .GroupBy(r => r.Title.ToLower())
                        .Select(g => g.First())
                        .OrderBy(r => r.ReleaseYear)
                        .ToList();
                }

                foreach (var recording in recordings.Results)
                {
                    var releaseYear = GetReleaseYear(recording);

                    if (yearFrom.HasValue && releaseYear.HasValue && releaseYear < yearFrom)
                        continue;
                    if (yearTo.HasValue && releaseYear.HasValue && releaseYear > yearTo)
                        continue;

                    results.Add(new MusicBrainzSongResult
                    {
                        MusicBrainzId = recording.Id.ToString(),
                        Title = recording.Title ?? string.Empty,
                        Duration = GetDurationSeconds(recording.Length),
                        ReleaseYear = releaseYear ?? 0,
                        IsrcCode = recording.Isrcs?.FirstOrDefault()
                    });
                }

                return results
                    .GroupBy(r => r.Title.ToLower())
                    .Select(g => g.First())
                    .OrderBy(r => r.ReleaseYear)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error("MUSICBRAINZ RECORDINGS ERROR: {Msg} | INNER: {Inner}", ex.Message, ex.InnerException?.Message);
                return new List<MusicBrainzSongResult>();
            }
        }

        public async Task<MusicBrainzArtistResult?> GetArtistByIdAsync(Guid artistMbid)
        {
            try
            {
                var artist = await query.LookupArtistAsync(artistMbid);

                if (artist == null)
                    return null;

                return new MusicBrainzArtistResult
                {
                    MusicBrainzId = artist.Id.ToString(),
                    Name = artist.Name ?? string.Empty,
                    CountryCode = artist.Area?.Iso31661Codes?.FirstOrDefault(),
                    CareerStart = ParseDate(artist.LifeSpan?.Begin?.ToString()),
                    CareerEnd = ParseDate(artist.LifeSpan?.End?.ToString())
                };
            }
            catch (Exception ex)
            {
                Log.Error("MUSICBRAINZ LOOKUP ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        private int GetDurationSeconds(TimeSpan? length)
        {
            if (!length.HasValue)
                return 0;

            return (int)length.Value.TotalSeconds;
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            if (DateTime.TryParse(dateString, out var date))
                return date;

            if (int.TryParse(dateString, out var year) && year > 1800 && year < 2100)
                return new DateTime(year, 1, 1);

            return null;
        }

        private int? GetReleaseYear(IRecording recording)
        {
            var firstReleaseDate = recording.FirstReleaseDate?.ToString();

            if (string.IsNullOrEmpty(firstReleaseDate))
                return null;

            if (firstReleaseDate.Length < 4)
                return null;

            if (int.TryParse(firstReleaseDate.Substring(0, 4), out var year))
                return year;

            return null;
        }
    }
}
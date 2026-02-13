using Jukebox_Backend.Data;
using Jukebox_Backend.DbHelpers;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Jukebox_Backend.Services
{
    public class PlaylistGenerationService : ServiceBase
    {
        private readonly MusicBrainzService musicBrainzService;
        private readonly TheAudioDBService audioDbService;
        private readonly YouTubeApiService youtubeService;

        // fixed: import max 30 new songs per artist, return 30 random
        private const int SongsPerArtist = 30;

        public PlaylistGenerationService(
            ApplicationDbContext context,
            MusicBrainzService musicBrainzService,
            TheAudioDBService audioDbService,
            YouTubeApiService youtubeService) : base(context)
        {
            this.musicBrainzService = musicBrainzService;
            this.audioDbService = audioDbService;
            this.youtubeService = youtubeService;
        }

        public async Task<Playlist?> GeneratePlaylistAsync(GeneratePlaylistRequest request, string userId)
        {
            try
            {
                var artistIds = request.Artists.Select(a => a.ArtistId).ToList();
                var artists = await _context.Artists
                    .Where(a => artistIds.Contains(a.ArtistId))
                    .ToListAsync();

                var artistNames = string.Join(", ", artists.Select(a => a.Name));

                var playlist = new Playlist
                {
                    Name = request.PlaylistName,
                    Category = request.Category ?? "Mixed",
                    Description = request.Description ?? $"Artists: {artistNames}",
                    IsGenerated = true,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Playlists.Add(playlist);
                await SaveAsync();

                var allSongs = new List<Song>();

                foreach (var artistSelection in request.Artists)
                {
                    var songs = await ImportAndGetSongsAsync(artistSelection.ArtistId);

                    if (songs != null)
                        allSongs.AddRange(songs);
                }

                if (allSongs.Count == 0)
                {
                    _context.Playlists.Remove(playlist);
                    await SaveAsync();
                    return null;
                }

                int order = 1;
                foreach (var song in allSongs)
                {
                    _context.PlaylistSongs.Add(new PlaylistSong
                    {
                        PlaylistId = playlist.PlaylistId,
                        SongId = song.SongId,
                        Order = order++,
                        AddedAt = DateTime.UtcNow
                    });
                }

                var distinctArtistIds = allSongs.Select(s => s.ArtistId).Distinct();
                foreach (var artistId in distinctArtistIds)
                {
                    _context.PlaylistArtists.Add(new PlaylistArtist
                    {
                        PlaylistId = playlist.PlaylistId,
                        ArtistId = artistId
                    });
                }

                playlist.SongsCount = allSongs.Count;
                await SaveAsync();

                Log.Information("PLAYLIST GENERATED: {Name} | {Count} songs | {Artists} artists",
                    playlist.Name, allSongs.Count, artistIds.Count);
                return playlist;
            }
            catch (Exception ex)
            {
                Log.Error("GENERATE ERROR: {Msg} | INNER: {Inner}", ex.Message, ex.InnerException?.Message);
                return null;
            }
        }

        // import max 30 new + return 30 random from db
        private async Task<List<Song>?> ImportAndGetSongsAsync(int artistId)
        {
            try
            {
                var artist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.ArtistId == artistId);

                if (artist == null) return null;

                var existingTitles = await _context.Songs
                    .Where(s => s.ArtistId == artistId)
                    .Select(s => s.Title)
                    .ToListAsync();

                Log.Information("IMPORT: {Artist} | {Existing} songs in db",
                    artist.Name, existingTitles.Count);

                int imported = 0;

                if (!string.IsNullOrEmpty(artist.MusicBrainzId))
                {
                    imported = await ImportFromTheAudioDbAsync(artist, existingTitles);

                    if (imported == 0)
                    {
                        imported = await ImportFromMusicBrainzAsync(artist, existingTitles);
                    }
                }

                Log.Information("IMPORT: {Artist} | {Imported} new | {Total} total in db",
                    artist.Name, imported, existingTitles.Count);

                // return 30 random from all songs in db for this artist
                var allSongs = await _context.Songs
                    .Where(s => s.ArtistId == artistId && !string.IsNullOrEmpty(s.YoutubeId))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(SongsPerArtist)
                    .ToListAsync();

                return allSongs.Count > 0 ? allSongs : null;
            }
            catch (Exception ex)
            {
                Log.Error("IMPORT SONGS ERROR: {Msg} | INNER: {Inner}",
                    ex.Message, ex.InnerException?.Message);

                var fallback = await _context.Songs
                    .Where(s => s.ArtistId == artistId && !string.IsNullOrEmpty(s.YoutubeId))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(SongsPerArtist)
                    .ToListAsync();

                return fallback.Count > 0 ? fallback : null;
            }
        }

        // audiodb: mvid lookup + albums + tracks, max 30 new
        private async Task<int> ImportFromTheAudioDbAsync(Artist artist, List<string> existingTitles)
        {
            try
            {
                var mbid = artist.MusicBrainzId!;

                var audioDbArtist = await audioDbService.GetArtistByMbidAsync(mbid);

                if (audioDbArtist == null || string.IsNullOrEmpty(audioDbArtist.IdArtist))
                {
                    Log.Information("AUDIODB: Artist not found by MBID for {Artist}", artist.Name);
                    return 0;
                }

                Log.Information("AUDIODB: Found artist {Name} (ID: {Id})",
                    audioDbArtist.StrArtist, audioDbArtist.IdArtist);

                // get all music videos -> map title -> youtubeId
                var mvids = await audioDbService.GetMusicVideosByArtistIdAsync(audioDbArtist.IdArtist);
                var videoLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var mv in mvids)
                {
                    var ytId = TheAudioDBService.ExtractYoutubeId(mv.StrMusicVid);
                    if (!string.IsNullOrEmpty(ytId) && !string.IsNullOrEmpty(mv.StrTrack))
                    {
                        videoLookup.TryAdd(mv.StrTrack, ytId);
                    }
                }

                Log.Information("AUDIODB: Found {Count} music videos for {Artist}",
                    videoLookup.Count, artist.Name);

                // get albums
                var albums = await audioDbService.GetAlbumsByArtistIdAsync(audioDbArtist.IdArtist);

                if (albums.Count == 0)
                {
                    Log.Information("AUDIODB: No albums found for {Artist}", artist.Name);
                    return 0;
                }

                Log.Information("AUDIODB: Found {Count} albums for {Artist}",
                    albums.Count, artist.Name);

                int totalImported = 0;
                int fromMvid = 0;
                int fromYoutube = 0;
                bool youtubeQuotaExhausted = false;

                foreach (var albumData in albums)
                {
                    if (totalImported >= SongsPerArtist) break;

                    if (string.IsNullOrEmpty(albumData.IdAlbum) ||
                        string.IsNullOrEmpty(albumData.StrAlbum))
                        continue;

                    var album = await GetOrCreateAlbumAsync(artist.ArtistId, albumData);

                    var tracks = await audioDbService.GetTracksByAlbumIdAsync(albumData.IdAlbum);

                    Log.Information("AUDIODB: Album '{Album}' has {Count} tracks",
                        albumData.StrAlbum, tracks.Count);

                    foreach (var track in tracks)
                    {
                        if (totalImported >= SongsPerArtist) break;

                        if (string.IsNullOrEmpty(track.StrTrack)) continue;
                        if (existingTitles.Any(t => SongTitleHelper.IsDuplicate(t, track.StrTrack))) continue;

                        // 1. youtube id from track field
                        var youtubeId = TheAudioDBService.ExtractYoutubeId(track.StrMusicVid);

                        // 2. youtube id from mvid lookup (FUZZY MATCH)
                        if (string.IsNullOrEmpty(youtubeId))
                        {
                            foreach (var kvp in videoLookup)
                            {
                                if (FuzzyTitleMatch(kvp.Key, track.StrTrack))
                                {
                                    youtubeId = kvp.Value;
                                    fromMvid++;
                                    break;
                                }
                            }
                        }

                        // 3. youtube api search (only if quota not exhausted)
                        if (string.IsNullOrEmpty(youtubeId) && !youtubeQuotaExhausted)
                        {
                            Log.Information("AUDIODB: No video for '{Track}', trying YouTube...",
                                track.StrTrack);

                            var ytResult = await youtubeService.SearchOfficialVideoAsync(
                                artist.Name, track.StrTrack);

                            if (ytResult != null)
                            {
                                youtubeId = ytResult.VideoId;
                                fromYoutube++;
                            }
                            else if (youtubeService.IsQuotaExhausted())
                            {
                                youtubeQuotaExhausted = true;
                                Log.Warning("YOUTUBE: quota exhausted, skipping remaining searches");
                            }
                        }

                        if (string.IsNullOrEmpty(youtubeId)) continue;

                        var song = new Song
                        {
                            Title = SongTitleHelper.Clean(track.StrTrack),
                            ArtistId = artist.ArtistId,
                            AlbumId = album.AlbumId,
                            Duration = TheAudioDBService.ParseDurationSeconds(track.IntDuration),
                            Genre = track.StrGenre ?? album.Genre ?? artist.Genre,
                            CountryCode = artist.CountryCode,
                            YoutubeId = youtubeId,
                            MusicBrainzId = track.StrMusicBrainzID,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Songs.Add(song);

                        if (await SaveAsync())
                        {
                            existingTitles.Add(SongTitleHelper.Clean(track.StrTrack));
                            totalImported++;
                        }

                        await Task.Delay(100);
                    }
                }

                artist.SongsCount = await _context.Songs.CountAsync(s => s.ArtistId == artist.ArtistId);
                await SaveAsync();

                Log.Information("AUDIODB: imported {Total} for {Artist} | {Mvid} from mvid | {Yt} from youtube",
                    totalImported, artist.Name, fromMvid, fromYoutube);

                return totalImported;
            }
            catch (Exception ex)
            {
                Log.Error("AUDIODB IMPORT ERROR: {Msg}", ex.Message);
                return 0;
            }
        }

        // musicbrainz + youtube fallback, max 30 new
        private async Task<int> ImportFromMusicBrainzAsync(Artist artist, List<string> existingTitles)
        {
            try
            {
                var mbid = Guid.Parse(artist.MusicBrainzId!);
                var recordings = await musicBrainzService.GetArtistRecordingsAsync(
                    mbid, null, null, limit: 100);

                Log.Information("MUSICBRAINZ: Found {Count} recordings for {Artist}",
                    recordings.Count, artist.Name);

                if (recordings.Count == 0) return 0;

                int totalImported = 0;

                foreach (var recording in recordings)
                {
                    if (totalImported >= SongsPerArtist) break;

                    if (existingTitles.Any(t => SongTitleHelper.IsDuplicate(t, recording.Title))) continue;

                    if (!string.IsNullOrEmpty(recording.IsrcCode))
                    {
                        var isrcExists = await _context.Songs
                            .AnyAsync(s => s.IsrcCode == recording.IsrcCode);
                        if (isrcExists) continue;
                    }

                    var ytResult = await youtubeService.SearchOfficialVideoAsync(
                        artist.Name, recording.Title);

                    if (ytResult == null) continue;

                    var song = new Song
                    {
                        Title = SongTitleHelper.Clean(recording.Title),
                        ArtistId = artist.ArtistId,
                        Duration = recording.Duration ?? 0,
                        ReleaseYear = recording.ReleaseYear,
                        Genre = artist.Genre ?? string.Empty,
                        CountryCode = artist.CountryCode,
                        YoutubeId = ytResult.VideoId,
                        IsrcCode = recording.IsrcCode,
                        MusicBrainzId = recording.MusicBrainzId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Songs.Add(song);

                    if (await SaveAsync())
                    {
                        existingTitles.Add(SongTitleHelper.Clean(recording.Title));
                        totalImported++;
                    }

                    await Task.Delay(100);
                }

                artist.SongsCount = await _context.Songs.CountAsync(s => s.ArtistId == artist.ArtistId);
                await SaveAsync();

                return totalImported;
            }
            catch (Exception ex)
            {
                Log.Error("MUSICBRAINZ IMPORT ERROR: {Msg}", ex.Message);
                return 0;
            }
        }

        // get or create album from audiodb data
        private async Task<Album> GetOrCreateAlbumAsync(int artistId, Models.Dto.ExternalApiData.TheAudioDbAlbumResult albumData)
        {
            var existing = await _context.Albums
                .FirstOrDefaultAsync(a => a.TheAudioDbId == albumData.IdAlbum);

            if (existing != null) return existing;

            var album = new Album
            {
                Title = albumData.StrAlbum ?? "Unknown Album",
                ArtistId = artistId,
                YearReleased = TheAudioDBService.ParseYear(albumData.IntYearReleased),
                Genre = albumData.StrGenre,
                Style = albumData.StrStyle,
                Mood = albumData.StrMood,
                Label = albumData.StrLabel,
                AlbumThumb = albumData.StrAlbumThumb,
                DescriptionEN = albumData.StrDescriptionEN,
                MusicBrainzId = albumData.StrMusicBrainzID,
                TheAudioDbId = albumData.IdAlbum,
                CreatedAt = DateTime.UtcNow
            };

            _context.Albums.Add(album);
            await SaveAsync();

            return album;
        }

        // import artist from musicbrainz + audiodb (used by SuperAdmin import endpoint)
        public async Task<Artist?> ImportArtistAsync(string artistName)
        {
            try
            {
                var existing = await _context.Artists
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == artistName.ToLower());

                if (existing != null) return existing;

                var mbResults = await musicBrainzService.SearchArtistsAsync(artistName, limit: 1);
                if (mbResults.Count == 0) return null;

                var mbArtist = mbResults[0];

                var audioDbArtist = !string.IsNullOrEmpty(mbArtist.MusicBrainzId)
                    ? await audioDbService.GetArtistByMbidAsync(mbArtist.MusicBrainzId)
                    : await audioDbService.SearchArtistAsync(artistName);

                var artist = new Artist
                {
                    Name = mbArtist.Name,
                    MusicBrainzId = mbArtist.MusicBrainzId,
                    CountryCode = audioDbArtist?.StrCountryCode ?? mbArtist.CountryCode,
                    CareerStart = mbArtist.CareerStart,
                    CareerEnd = mbArtist.CareerEnd,
                    IsActive = mbArtist.CareerEnd == null,
                    Photo = audioDbArtist?.StrArtistThumb ?? "default.jpg",
                    Biography = audioDbArtist?.StrBiographyEN,
                    Genre = audioDbArtist?.StrGenre,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Artists.Add(artist);
                await SaveAsync();

                Log.Information("ARTIST IMPORTED: {Name} | MBID: {Mbid}",
                    artist.Name, artist.MusicBrainzId);

                return artist;
            }
            catch (Exception ex)
            {
                Log.Error("IMPORT ARTIST ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // search musicbrainz WITHOUT importing — just return results for dropdown
        public async Task<List<object>> SearchMusicBrainzAsync(string query, int limit = 10)
        {
            var results = new List<object>();

            try
            {
                // capitalize each word: i have some problems with the search quesry some case sensitive issues
                var normalizedQuery = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(query.ToLower());
                var mbResults = await musicBrainzService.SearchArtistsAsync(normalizedQuery, limit);
                if (mbResults.Count == 0) return results;

                // filter low-score results and sort best first
                mbResults = mbResults
                    .Where(m => m.Score >= 50)
                    .OrderByDescending(m => m.Score)
                    .ToList();

                if (mbResults.Count == 0) return results;

                var mbIds = mbResults
                    .Where(m => !string.IsNullOrEmpty(m.MusicBrainzId))
                    .Select(m => m.MusicBrainzId)
                    .ToList();

                var existingArtists = await _context.Artists
                    .Where(a => a.MusicBrainzId != null && mbIds.Contains(a.MusicBrainzId))
                    .ToListAsync();

                var existingByMbId = existingArtists.ToDictionary(a => a.MusicBrainzId!, a => a);

                foreach (var mb in mbResults)
                {
                    if (string.IsNullOrEmpty(mb.MusicBrainzId)) continue;

                    if (existingByMbId.TryGetValue(mb.MusicBrainzId, out var existing))
                    {
                        results.Add(new
                        {
                            artistId = (int?)existing.ArtistId,
                            name = existing.Name,
                            photo = existing.Photo,
                            genre = existing.Genre,
                            countryCode = existing.CountryCode,
                            musicBrainzId = existing.MusicBrainzId
                        });
                    }
                    else
                    {
                        results.Add(new
                        {
                            artistId = (int?)null,
                            name = mb.Name,
                            photo = (string?)null,
                            genre = (string?)null,
                            countryCode = mb.CountryCode,
                            musicBrainzId = mb.MusicBrainzId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SEARCH-MUSICBRAINZ ERROR: {Msg}", ex.Message);
            }

            return results;
        }

        // import ONE artist by MusicBrainzId — called when user SELECTS from dropdown
        public async Task<Artist?> ImportByMusicBrainzIdAsync(string musicBrainzId)
        {
            try
            {
                var existing = await _context.Artists
                    .FirstOrDefaultAsync(a => a.MusicBrainzId == musicBrainzId);

                if (existing != null) return existing;

                var mbArtist = await musicBrainzService.GetArtistByIdAsync(Guid.Parse(musicBrainzId));
                if (mbArtist == null) return null;

                var audioDbArtist = await audioDbService.GetArtistByMbidAsync(musicBrainzId);

                var artist = new Artist
                {
                    Name = mbArtist.Name,
                    MusicBrainzId = mbArtist.MusicBrainzId,
                    CountryCode = audioDbArtist?.StrCountryCode ?? mbArtist.CountryCode,
                    CareerStart = mbArtist.CareerStart,
                    CareerEnd = mbArtist.CareerEnd,
                    IsActive = mbArtist.CareerEnd == null,
                    Photo = audioDbArtist?.StrArtistThumb ?? "default.jpg",
                    Biography = audioDbArtist?.StrBiographyEN,
                    Genre = audioDbArtist?.StrGenre,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Artists.Add(artist);
                await SaveAsync();

                Log.Information("ARTIST IMPORTED ON SELECT: {Name} | MBID: {Mbid}",
                    artist.Name, artist.MusicBrainzId);

                return artist;
            }
            catch (Exception ex)
            {
                Log.Error("IMPORT BY MBID ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // fuzzy match: normalize titles removing parentheses, brackets, feat, punctuation
        private static string NormalizeTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";

            var normalized = title.ToLower().Trim();

            // Remove everything in parentheses: (From 8 Mile), (feat. X), (Remix), etc.
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\(.*?\)", "");
            // Remove everything in brackets: [Explicit], [Live], etc.
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\[.*?\]", "");
            // Remove feat./ft./featuring
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b(feat\.?|ft\.?|featuring)\b.*", "");
            // Remove punctuation
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9\s]", "");
            // Collapse spaces
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }

        private static bool FuzzyTitleMatch(string title1, string title2)
        {
            var norm1 = NormalizeTitle(title1);
            var norm2 = NormalizeTitle(title2);

            if (string.IsNullOrEmpty(norm1) || string.IsNullOrEmpty(norm2)) return false;

            // Exact match after normalization
            if (norm1 == norm2) return true;

            // One contains the other
            if (norm1.Contains(norm2) || norm2.Contains(norm1)) return true;

            // Word overlap: if 2+ words match
            var words1 = norm1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var words2 = norm2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var common = words1.Intersect(words2).Count();

            return common >= 2 || (common >= 1 && (words1.Count == 1 || words2.Count == 1));
        }
    }
}
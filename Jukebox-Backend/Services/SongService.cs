using Jukebox_Backend.Data;
using Jukebox_Backend.DbHelpers;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class SongService : ServiceBase
    {
        public SongService(ApplicationDbContext context) : base(context) { }

        // search songs with pagination
        public async Task<PaginatedResponse<SongResponse>> SearchAsync(SearchSongRequest request)
        {
            try
            {
                var query = _context.Songs
                    .AsNoTracking()
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Where(s => !s.IsDeleted);

                // filters
                if (!string.IsNullOrEmpty(request.Title))
                    query = query.Where(s => s.Title.Contains(request.Title));

                if (request.ArtistId.HasValue)
                    query = query.Where(s => s.ArtistId == request.ArtistId.Value);

                if (!string.IsNullOrEmpty(request.Genre))
                    query = query.Where(s => s.Genre != null && s.Genre.Contains(request.Genre));

                if (!string.IsNullOrEmpty(request.CountryCode))
                    query = query.Where(s => s.CountryCode == request.CountryCode);

                if (request.ReleaseYearFrom.HasValue)
                    query = query.Where(s => s.ReleaseYear >= request.ReleaseYearFrom.Value);

                if (request.ReleaseYearTo.HasValue)
                    query = query.Where(s => s.ReleaseYear <= request.ReleaseYearTo.Value);

                var totalItems = await query.CountAsync();

                var songs = await query
                    .OrderBy(s => s.Title)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(s => new SongResponse
                    {
                        SongId = s.SongId,
                        Title = s.Title,
                        CountryCode = s.CountryCode,
                        Duration = s.Duration,
                        ReleaseYear = s.ReleaseYear,
                        Genre = s.Genre,
                        SongsPlayed = s.SongsPlayed,
                        YoutubeId = s.YoutubeId,
                        MusicBrainzId = s.MusicBrainzId,
                        IsrcCode = s.IsrcCode,
                        CreatedAt = s.CreatedAt,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist != null ? s.Artist.Name : string.Empty,
                        AlbumId = s.AlbumId,
                        AlbumTitle = s.Album != null ? s.Album.Title : null
                    })
                    .ToListAsync();

                return new PaginatedResponse<SongResponse>
                {
                    Items = songs,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<SongResponse>();
            }
        }

        // get song by id
        public async Task<SongResponse?> GetByIdAsync(int id)
        {
            try
            {
                var song = await _context.Songs
                    .AsNoTracking()
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .FirstOrDefaultAsync(s => s.SongId == id && !s.IsDeleted);

                if (song is null) return null;

                return new SongResponse
                {
                    SongId = song.SongId,
                    Title = song.Title,
                    CountryCode = song.CountryCode,
                    Duration = song.Duration,
                    ReleaseYear = song.ReleaseYear,
                    Genre = song.Genre,
                    SongsPlayed = song.SongsPlayed,
                    YoutubeId = song.YoutubeId,
                    MusicBrainzId = song.MusicBrainzId,
                    IsrcCode = song.IsrcCode,
                    CreatedAt = song.CreatedAt,
                    ArtistId = song.ArtistId,
                    ArtistName = song.Artist != null ? song.Artist.Name : string.Empty,
                    AlbumId = song.AlbumId,
                    AlbumTitle = song.Album != null ? song.Album.Title : null
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // create song
        public async Task<SongResponse?> CreateAsync(CreateSongRequest request)
        {
            try
            {
                var artistExists = await _context.Artists
                    .AnyAsync(a => a.ArtistId == request.ArtistId && !a.IsDeleted);

                if (!artistExists) return null;

                // clean song title
                request.Title = SongTitleHelper.Clean(request.Title);

                var song = new Song
                {
                    Title = request.Title,
                    CountryCode = request.CountryCode,
                    Duration = request.Duration,
                    ReleaseYear = request.ReleaseYear,
                    Genre = request.Genre,
                    YoutubeId = request.YoutubeId,
                    MusicBrainzId = request.MusicBrainzId,
                    IsrcCode = request.IsrcCode,
                    ArtistId = request.ArtistId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Songs.Add(song);
                bool saved = await SaveAsync();

                if (!saved) return null;

                var artist = await _context.Artists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArtistId == request.ArtistId);

                return new SongResponse
                {
                    SongId = song.SongId,
                    Title = song.Title,
                    CountryCode = song.CountryCode,
                    Duration = song.Duration,
                    ReleaseYear = song.ReleaseYear,
                    Genre = song.Genre,
                    SongsPlayed = song.SongsPlayed,
                    YoutubeId = song.YoutubeId,
                    MusicBrainzId = song.MusicBrainzId,
                    IsrcCode = song.IsrcCode,
                    CreatedAt = song.CreatedAt,
                    ArtistId = song.ArtistId,
                    ArtistName = artist?.Name ?? string.Empty,
                    AlbumId = song.AlbumId,
                    AlbumTitle = null
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // update song
        public async Task<SongResponse?> UpdateAsync(int id, UpdateSongRequest request)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .FirstOrDefaultAsync(s => s.SongId == id && !s.IsDeleted);

                if (song is null) return null;

                if (request.Title is not null) song.Title = request.Title;
                if (request.CountryCode is not null) song.CountryCode = request.CountryCode;
                if (request.Duration.HasValue) song.Duration = request.Duration.Value;
                if (request.ReleaseYear.HasValue) song.ReleaseYear = request.ReleaseYear.Value;
                if (request.Genre is not null) song.Genre = request.Genre;
                if (request.YoutubeId is not null) song.YoutubeId = request.YoutubeId;
                if (request.MusicBrainzId is not null) song.MusicBrainzId = request.MusicBrainzId;
                if (request.IsrcCode is not null) song.IsrcCode = request.IsrcCode;

                if (request.ArtistId.HasValue)
                {
                    var artistExists = await _context.Artists
                        .AnyAsync(a => a.ArtistId == request.ArtistId.Value && !a.IsDeleted);

                    if (!artistExists) return null;

                    song.ArtistId = request.ArtistId.Value;
                }

                bool saved = await SaveAsync();

                if (!saved) return null;

                // reload artist name if changed
                if (request.ArtistId.HasValue)
                {
                    song = await _context.Songs
                        .AsNoTracking()
                        .Include(s => s.Artist)
                        .Include(s => s.Album)
                        .FirstOrDefaultAsync(s => s.SongId == id);
                }

                return new SongResponse
                {
                    SongId = song!.SongId,
                    Title = song.Title,
                    CountryCode = song.CountryCode,
                    Duration = song.Duration,
                    ReleaseYear = song.ReleaseYear,
                    Genre = song.Genre,
                    SongsPlayed = song.SongsPlayed,
                    YoutubeId = song.YoutubeId,
                    MusicBrainzId = song.MusicBrainzId,
                    IsrcCode = song.IsrcCode,
                    CreatedAt = song.CreatedAt,
                    ArtistId = song.ArtistId,
                    ArtistName = song.Artist?.Name ?? string.Empty,
                    AlbumId = song.AlbumId,
                    AlbumTitle = song.Album?.Title
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // soft delete song
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var song = await _context.Songs
                    .FirstOrDefaultAsync(s => s.SongId == id && !s.IsDeleted);

                if (song is null) return false;

                song.IsDeleted = true;
                song.DeletedAt = DateTime.UtcNow;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        // increment play count for song and artist
        public async Task<bool> IncrementPlayCountAsync(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(s => s.SongId == id && !s.IsDeleted);

            if (song is null) return false;

            song.SongsPlayed++;

            // increment total played for artist
            if (song.Artist != null)
            {
                song.Artist.TotalPlayed++;
            }

            return await SaveAsync();
        }
    }
}
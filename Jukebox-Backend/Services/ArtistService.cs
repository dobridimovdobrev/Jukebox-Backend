using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class ArtistService : ServiceBase
    {
        public ArtistService(ApplicationDbContext context) : base(context) { }

        // search artists with pagination
        public async Task<PaginatedResponse<ArtistResponse>> SearchAsync(SearchArtistRequest request)
        {
            try
            {
                var query = _context.Artists
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted);

                // filters
                if (!string.IsNullOrEmpty(request.Name))
                    query = query.Where(a => a.Name.Contains(request.Name));

                if (!string.IsNullOrEmpty(request.CountryCode))
                    query = query.Where(a => a.CountryCode == request.CountryCode);

                if (!string.IsNullOrEmpty(request.Genre))
                    query = query.Where(a => a.Genre != null && a.Genre.Contains(request.Genre));

                if (request.IsActive.HasValue)
                    query = query.Where(a => a.IsActive == request.IsActive.Value);

                if (request.CareerStartYear.HasValue)
                    query = query.Where(a => a.CareerStart.HasValue && a.CareerStart.Value.Year >= request.CareerStartYear.Value);

                if (request.CareerEndYear.HasValue)
                    query = query.Where(a => a.CareerEnd.HasValue && a.CareerEnd.Value.Year <= request.CareerEndYear.Value);

                var totalItems = await query.CountAsync();

                var artists = await query
                    .OrderBy(a => a.Name)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(a => new ArtistResponse
                    {
                        ArtistId = a.ArtistId,
                        Name = a.Name,
                        Photo = a.Photo,
                        CountryCode = a.CountryCode,
                        Genre = a.Genre,
                        CareerStart = a.CareerStart.HasValue ? a.CareerStart.Value.Year : null,
                        CareerEnd = a.CareerEnd.HasValue ? a.CareerEnd.Value.Year : null,
                        SongsCount = a.SongsCount,
                        YoutubeChannelId = a.YoutubeChannelId,
                        MusicBrainzId = a.MusicBrainzId,
                        IsrcCode = a.IsrcCode,
                        Biography = a.Biography,
                        CreatedAt = a.CreatedAt,
                        IsActive = a.IsActive
                    })
                    .ToListAsync();

                return new PaginatedResponse<ArtistResponse>
                {
                    Items = artists,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<ArtistResponse>();
            }
        }

        // get artist by id
        public async Task<ArtistResponse?> GetByIdAsync(int id)
        {
            try
            {
                var artist = await _context.Artists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArtistId == id && !a.IsDeleted);

                if (artist is null) return null;

                return new ArtistResponse
                {
                    ArtistId = artist.ArtistId,
                    Name = artist.Name,
                    Photo = artist.Photo,
                    CountryCode = artist.CountryCode,
                    Genre = artist.Genre,
                    CareerStart = artist.CareerStart?.Year,
                    CareerEnd = artist.CareerEnd?.Year,
                    SongsCount = artist.SongsCount,
                    YoutubeChannelId = artist.YoutubeChannelId,
                    MusicBrainzId = artist.MusicBrainzId,
                    IsrcCode = artist.IsrcCode,
                    Biography = artist.Biography,
                    CreatedAt = artist.CreatedAt,
                    IsActive = artist.IsActive
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // create artist manually
        public async Task<ArtistResponse?> CreateAsync(CreateArtistRequest request)
        {
            try
            {
                var exists = await _context.Artists
                    .AnyAsync(a => a.Name == request.Name && !a.IsDeleted);

                if (exists) return null;

                var artist = new Artist
                {
                    Name = request.Name,
                    Photo = request.Photo,
                    CountryCode = request.CountryCode,
                    Genre = request.Genre,
                    CareerStart = request.CareerStart,
                    CareerEnd = request.CareerEnd,
                    YoutubeChannelId = request.YoutubeChannelId,
                    MusicBrainzId = request.MusicBrainzId,
                    IsrcCode = request.IsrcCode,
                    Biography = request.Biography,
                    IsActive = request.CareerEnd == null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Artists.Add(artist);
                bool saved = await SaveAsync();

                if (!saved) return null;

                return new ArtistResponse
                {
                    ArtistId = artist.ArtistId,
                    Name = artist.Name,
                    Photo = artist.Photo,
                    CountryCode = artist.CountryCode,
                    Genre = artist.Genre,
                    CareerStart = artist.CareerStart?.Year,
                    CareerEnd = artist.CareerEnd?.Year,
                    SongsCount = artist.SongsCount,
                    YoutubeChannelId = artist.YoutubeChannelId,
                    MusicBrainzId = artist.MusicBrainzId,
                    IsrcCode = artist.IsrcCode,
                    Biography = artist.Biography,
                    CreatedAt = artist.CreatedAt,
                    IsActive = artist.IsActive
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // update artist
        public async Task<ArtistResponse?> UpdateAsync(int id, UpdateArtistRequest request)
        {
            try
            {
                var artist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.ArtistId == id && !a.IsDeleted);

                if (artist is null) return null;

                if (request.Name is not null) artist.Name = request.Name;
                if (request.Photo is not null) artist.Photo = request.Photo;
                if (request.CountryCode is not null) artist.CountryCode = request.CountryCode;
                if (request.Genre is not null) artist.Genre = request.Genre;
                if (request.CareerStart.HasValue) artist.CareerStart = request.CareerStart;
                if (request.CareerEnd.HasValue) artist.CareerEnd = request.CareerEnd;
                if (request.YoutubeChannelId is not null) artist.YoutubeChannelId = request.YoutubeChannelId;
                if (request.MusicBrainzId is not null) artist.MusicBrainzId = request.MusicBrainzId;
                if (request.IsrcCode is not null) artist.IsrcCode = request.IsrcCode;
                if (request.Biography is not null) artist.Biography = request.Biography;
                if (request.IsActive.HasValue) artist.IsActive = request.IsActive.Value;

                bool saved = await SaveAsync();

                if (!saved) return null;

                return new ArtistResponse
                {
                    ArtistId = artist.ArtistId,
                    Name = artist.Name,
                    Photo = artist.Photo,
                    CountryCode = artist.CountryCode,
                    Genre = artist.Genre,
                    CareerStart = artist.CareerStart?.Year,
                    CareerEnd = artist.CareerEnd?.Year,
                    SongsCount = artist.SongsCount,
                    YoutubeChannelId = artist.YoutubeChannelId,
                    MusicBrainzId = artist.MusicBrainzId,
                    IsrcCode = artist.IsrcCode,
                    Biography = artist.Biography,
                    CreatedAt = artist.CreatedAt,
                    IsActive = artist.IsActive
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // soft delete artist
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var artist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.ArtistId == id && !a.IsDeleted);

                if (artist is null) return false;

                artist.IsDeleted = true;
                artist.DeletedAt = DateTime.UtcNow;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class PlaylistService : ServiceBase
    {
        private readonly PlaylistGenerationService _generationService;

        public PlaylistService(ApplicationDbContext context, PlaylistGenerationService generationService) : base(context)
        {
            _generationService = generationService;
        }

        // get all playlists by user
        public async Task<List<PlaylistResponse>> GetByUserAsync(string userId)
        {
            try
            {
                return await _context.Playlists
                    .AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .Select(p => new PlaylistResponse
                    {
                        PlaylistId = p.PlaylistId,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category ?? string.Empty,
                        SongsCount = p.SongsCount,
                        IsGenerated = p.IsGenerated,
                        CreatedAt = p.CreatedAt,
                        UserId = p.UserId,
                        UserFullName = p.User != null ? p.User.FirstName + " " + p.User.LastName : null
                    })
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<PlaylistResponse>();
            }
        }

        // get all playlists (admin)
        public async Task<List<PlaylistResponse>> GetAllAsync()
        {
            try
            {
                return await _context.Playlists
                    .AsNoTracking()
                    .Include(p => p.User)
                    .Select(p => new PlaylistResponse
                    {
                        PlaylistId = p.PlaylistId,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category ?? string.Empty,
                        SongsCount = p.SongsCount,
                        IsGenerated = p.IsGenerated,
                        CreatedAt = p.CreatedAt,
                        UserId = p.UserId,
                        UserFullName = p.User != null ? p.User.FirstName + " " + p.User.LastName : null
                    })
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<PlaylistResponse>();
            }
        }

        public async Task<PaginatedResponse<PlaylistResponse>> SearchAsync(SearchPlaylistRequest request)
        {
            try
            {
                var query = _context.Playlists
                    .AsNoTracking()
                    .Include(p => p.User)
                    .Include(p => p.PlaylistArtists.Where(pa => !pa.IsDeleted))
                        .ThenInclude(pa => pa.Artist)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Name))
                    query = query.Where(p => p.Name.Contains(request.Name));

                if (!string.IsNullOrEmpty(request.Category))
                    query = query.Where(p => p.Category != null && p.Category.Contains(request.Category));

                if (request.IsGenerated.HasValue)
                    query = query.Where(p => p.IsGenerated == request.IsGenerated.Value);

                var totalItems = await query.CountAsync();

                var playlists = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new PlaylistResponse
                    {
                        PlaylistId = p.PlaylistId,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category ?? string.Empty,
                        SongsCount = p.SongsCount,
                        IsGenerated = p.IsGenerated,
                        CreatedAt = p.CreatedAt,
                        UserId = p.UserId,
                        UserFullName = p.User != null ? p.User.FirstName + " " + p.User.LastName : null,
                        Artists = p.PlaylistArtists
                            .Select(pa => new PlaylistArtistInfo
                            {
                                ArtistId = pa.ArtistId,
                                ArtistName = pa.Artist != null ? pa.Artist.Name : string.Empty,
                                CareerStart = pa.Artist != null && pa.Artist.CareerStart.HasValue ? pa.Artist.CareerStart.Value.Year : null,
                                CareerEnd = pa.Artist != null && pa.Artist.CareerEnd.HasValue ? pa.Artist.CareerEnd.Value.Year : null
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return new PaginatedResponse<PlaylistResponse>
                {
                    Items = playlists,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<PlaylistResponse>();
            }
        }

        // get playlist by id with songs and artists
        public async Task<PlaylistResponse?> GetByIdAsync(int id)
        {
            try
            {
                var playlist = await _context.Playlists
                    .AsNoTracking()
                    .Include(p => p.User)
                    .Include(p => p.PlaylistSongs.Where(ps => !ps.IsDeleted))
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s!.Artist)
                    .Include(p => p.PlaylistArtists.Where(pa => !pa.IsDeleted))
                        .ThenInclude(pa => pa.Artist)
                    .FirstOrDefaultAsync(p => p.PlaylistId == id);

                if (playlist is null) return null;

                return new PlaylistResponse
                {
                    PlaylistId = playlist.PlaylistId,
                    Name = playlist.Name,
                    Description = playlist.Description,
                    Category = playlist.Category ?? string.Empty,
                    SongsCount = playlist.SongsCount,
                    IsGenerated = playlist.IsGenerated,
                    CreatedAt = playlist.CreatedAt,
                    UserId = playlist.UserId,
                    UserFullName = playlist.User != null ? playlist.User.FirstName + " " + playlist.User.LastName : null,
                    Songs = playlist.PlaylistSongs
                        .OrderBy(ps => ps.Order)
                        .Select(ps => new PlaylistSongInfo
                        {
                            SongId = ps.SongId,
                            Title = ps.Song?.Title ?? string.Empty,
                            ArtistName = ps.Song?.Artist?.Name ?? string.Empty,
                            Duration = ps.Song?.Duration ?? 0,
                            YoutubeId = ps.Song?.YoutubeId ?? string.Empty,
                            Order = ps.Order
                        })
                        .ToList(),
                    Artists = playlist.PlaylistArtists
                        .Select(pa => new PlaylistArtistInfo
                        {
                            ArtistId = pa.ArtistId,
                            ArtistName = pa.Artist?.Name ?? string.Empty,
                            CareerStart = pa.Artist?.CareerStart?.Year,
                            CareerEnd = pa.Artist?.CareerEnd?.Year
                        })
                        .ToList()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // create empty playlist
        public async Task<PlaylistResponse?> CreateAsync(CreatePlaylistRequest request, string userId)
        {
            try
            {
                var playlist = new Playlist
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    SongsCount = request.SongsCount,
                    IsGenerated = false,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Playlists.Add(playlist);
                bool saved = await SaveAsync();

                if (!saved) return null;

                return new PlaylistResponse
                {
                    PlaylistId = playlist.PlaylistId,
                    Name = playlist.Name,
                    Description = playlist.Description,
                    Category = playlist.Category ?? string.Empty,
                    SongsCount = playlist.SongsCount,
                    IsGenerated = playlist.IsGenerated,
                    CreatedAt = playlist.CreatedAt,
                    UserId = playlist.UserId
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // generate playlist from quiz results (import songs from APIs)
        public async Task<PlaylistResponse?> GenerateAsync(GeneratePlaylistRequest request, string userId)
        {
            try
            {
                var playlist = await _generationService.GeneratePlaylistAsync(request, userId);

                if (playlist is null) return null;

                // reload with full details
                return await GetByIdAsync(playlist.PlaylistId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // update playlist
        public async Task<PlaylistResponse?> UpdateAsync(int id, UpdatePlaylistRequest request, string userId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

                if (playlist is null) return null;

                if (request.Name is not null) playlist.Name = request.Name;
                if (request.Description is not null) playlist.Description = request.Description;
                if (request.Category is not null) playlist.Category = request.Category;

                bool saved = await SaveAsync();

                if (!saved) return null;

                return await GetByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // add song to playlist
        public async Task<bool> AddSongAsync(int playlistId, int songId, string userId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

                if (playlist is null) return false;

                var songExists = await _context.Songs
                    .AnyAsync(s => s.SongId == songId && !s.IsDeleted);

                if (!songExists) return false;

                var alreadyAdded = await _context.PlaylistSongs
                    .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId && !ps.IsDeleted);

                if (alreadyAdded) return false;

                // get next order number
                var maxOrder = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == playlistId && !ps.IsDeleted)
                    .MaxAsync(ps => (int?)ps.Order) ?? 0;

                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId,
                    Order = maxOrder + 1,
                    AddedAt = DateTime.UtcNow
                };

                _context.PlaylistSongs.Add(playlistSong);

                // update songs count
                playlist.SongsCount = (playlist.SongsCount ?? 0) + 1;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        // remove song from playlist
        public async Task<bool> RemoveSongAsync(int playlistId, int songId, string userId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

                if (playlist is null) return false;

                var playlistSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId && !ps.IsDeleted);

                if (playlistSong is null) return false;

                playlistSong.IsDeleted = true;
                playlistSong.DeletedAt = DateTime.UtcNow;

                // update songs count
                if (playlist.SongsCount > 0)
                    playlist.SongsCount--;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        // soft delete playlist
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

                if (playlist is null) return false;

                playlist.IsDeleted = true;
                playlist.DeletedAt = DateTime.UtcNow;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
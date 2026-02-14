using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class UserService : ServiceBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context)
        {
            _userManager = userManager;
        }

        // get all users (admin)
        public async Task<List<UserResponse>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Cast<ApplicationUser>()
                    .Where(u => u.IsActive)
                    .Select(u => new UserResponse
                    {
                        UserId = u.Id,
                        Email = u.Email ?? string.Empty,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Gender = u.Gender,
                        Country = u.Country,
                        Birthday = u.Birthday,
                        CreatedAt = u.CreatedAt,
                        Coins = u.Coins,
                        TotalSongsPlayed = u.TotalSongsPlayed,
                        IsActive = u.IsActive
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserResponse>();
            }
        }

        public async Task<PaginatedResponse<UserResponse>> SearchAsync(SearchUserRequest request)
        {
            try
            {
                var query = _context.Users
                    .AsNoTracking()
                    .Cast<ApplicationUser>()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Name))
                    query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(request.Name));

                if (!string.IsNullOrEmpty(request.Email))
                    query = query.Where(u => u.Email != null && u.Email.Contains(request.Email));

                if (!string.IsNullOrEmpty(request.Country))
                    query = query.Where(u => u.Country == request.Country);

                if (request.IsActive.HasValue)
                    query = query.Where(u => u.IsActive == request.IsActive.Value);

                var totalItems = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserResponse
                    {
                        UserId = u.Id,
                        Email = u.Email ?? string.Empty,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Gender = u.Gender,
                        Country = u.Country,
                        Birthday = u.Birthday,
                        CreatedAt = u.CreatedAt,
                        Coins = u.Coins,
                        TotalSongsPlayed = u.TotalSongsPlayed,
                        IsActive = u.IsActive
                    })
                     .ToListAsync();

                // roles in front end in settings page / users
                foreach (var userResp in users)
                {
                    var appUser = await _userManager.FindByIdAsync(userResp.UserId);
                    if (appUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(appUser);
                        userResp.Role = roles.FirstOrDefault() ?? "User";
                    }
                }

                return new PaginatedResponse<UserResponse>
                {
                    Items = users,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<UserResponse>();
            }
        }

        // toggle user active status (admin)
        public async Task<bool> ToggleActiveAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null) return false;

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // get user profile
        public async Task<UserResponse?> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Country = user.Country,
                Birthday = user.Birthday,
                CreatedAt = user.CreatedAt,
                Coins = user.Coins,
                TotalSongsPlayed = user.TotalSongsPlayed,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "User"
            };
        }

        // update user profile
        public async Task<UserResponse?> UpdateAsync(string userId, UpdateUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null) return null;

                if (request.FirstName is not null) user.FirstName = request.FirstName;
                if (request.LastName is not null) user.LastName = request.LastName;
                if (request.Gender is not null) user.Gender = request.Gender;
                if (request.Country is not null) user.Country = request.Country;
                if (request.Birthday.HasValue) user.Birthday = request.Birthday.Value;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded) return null;

                return new UserResponse
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Country = user.Country,
                    Birthday = user.Birthday,
                    CreatedAt = user.CreatedAt,
                    Coins = user.Coins,
                    TotalSongsPlayed = user.TotalSongsPlayed,
                    IsActive = user.IsActive
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // deactivate user (admin)
        public async Task<bool> DeactivateAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null) return false;

                user.IsActive = false;

                var result = await _userManager.UpdateAsync(user);

                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // spend coins (when user plays songs)
        public async Task<bool> SpendCoinsAsync(string userId, int amount)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null || user.Coins < amount) return false;

                user.Coins -= amount;
                user.TotalSongsPlayed++;

                var result = await _userManager.UpdateAsync(user);

                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
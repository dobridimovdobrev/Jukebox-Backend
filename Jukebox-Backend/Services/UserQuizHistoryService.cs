using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class UserQuizHistoryService : ServiceBase
    {
        public UserQuizHistoryService(ApplicationDbContext context) : base(context) { }

        // get history by user
        public async Task<List<UserQuizHistoryResponse>> GetByUserAsync(string userId)
        {
            try
            {
                return await _context.UserQuizHistories
                    .AsNoTracking()
                    .Where(h => h.UserId == userId)
                    .Include(h => h.User)
                    .Select(h => new UserQuizHistoryResponse
                    {
                        UserQuizId = h.UserQuizId,
                        UserId = h.UserId,
                        UserFullName = h.User != null ? h.User.FirstName + " " + h.User.LastName : null,
                        Category = h.Category,
                        Difficulty = h.Difficulty,
                        CorrectAnswers = h.CorrectAnswers,
                        WrongAnswers = h.WrongAnswers,
                        CoinsEarned = h.CoinsEarned,
                        PlayedAt = h.PlayedAt
                    })
                    .OrderByDescending(h => h.PlayedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserQuizHistoryResponse>();
            }
        }

        // get all history (admin)
        public async Task<List<UserQuizHistoryResponse>> GetAllAsync()
        {
            try
            {
                return await _context.UserQuizHistories
                    .AsNoTracking()
                    .Include(h => h.User)
                    .Select(h => new UserQuizHistoryResponse
                    {
                        UserQuizId = h.UserQuizId,
                        UserId = h.UserId,
                        UserFullName = h.User != null ? h.User.FirstName + " " + h.User.LastName : null,
                        Category = h.Category,
                        Difficulty = h.Difficulty,
                        CorrectAnswers = h.CorrectAnswers,
                        WrongAnswers = h.WrongAnswers,
                        CoinsEarned = h.CoinsEarned,
                        PlayedAt = h.PlayedAt
                    })
                    .OrderByDescending(h => h.PlayedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserQuizHistoryResponse>();
            }
        }

        // create history entry + update user coins
        public async Task<UserQuizHistoryResponse?> CreateAsync(CreateUserQuizHistoryRequest request, string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user is null) return null;

                var history = new UserQuizHistory
                {
                    UserId = userId,
                    Category = request.Category,
                    Difficulty = request.Difficulty,
                    CorrectAnswers = request.CorrectAnswers,
                    WrongAnswers = request.WrongAnswers,
                    CoinsEarned = request.CoinsEarned,
                    PlayedAt = DateTime.UtcNow
                };

                _context.UserQuizHistories.Add(history);

                // update user coins
                user.Coins += request.CoinsEarned;

                bool saved = await SaveAsync();

                if (!saved) return null;

                return new UserQuizHistoryResponse
                {
                    UserQuizId = history.UserQuizId,
                    UserId = history.UserId,
                    UserFullName = user.FirstName + " " + user.LastName,
                    Category = history.Category,
                    Difficulty = history.Difficulty,
                    CorrectAnswers = history.CorrectAnswers,
                    WrongAnswers = history.WrongAnswers,
                    CoinsEarned = history.CoinsEarned,
                    PlayedAt = history.PlayedAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
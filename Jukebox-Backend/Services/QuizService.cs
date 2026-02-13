using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class QuizService : ServiceBase
    {
        public QuizService(ApplicationDbContext context) : base(context) { }

        // get all quizzes
        public async Task<List<QuizResponse>> GetAllAsync()
        {
            try
            {
                return await _context.Quizzes
                    .AsNoTracking()
                    .Where(q => !q.IsDeleted)
                    .Select(q => new QuizResponse
                    {
                        QuizId = q.QuizId,
                        Question = q.Question,
                        CorrectAnswer = q.CorrectAnswer,
                        IncorrectAnswers = q.IncorrectAnswers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Category = q.Category,
                        Difficulty = q.Difficulty,
                        CreatedAt = q.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<QuizResponse>();
            }
        }

        public async Task<PaginatedResponse<QuizResponse>> SearchAsync(SearchQuizRequest request)
        {
            try
            {
                var query = _context.Quizzes
                    .AsNoTracking()
                    .Where(q => !q.IsDeleted);

                if (!string.IsNullOrEmpty(request.Question))
                    query = query.Where(q => q.Question.Contains(request.Question));

                if (!string.IsNullOrEmpty(request.Category))
                    query = query.Where(q => q.Category == request.Category);

                if (!string.IsNullOrEmpty(request.Difficulty))
                    query = query.Where(q => q.Difficulty == request.Difficulty);

                var totalItems = await query.CountAsync();

                var quizzes = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(q => new QuizResponse
                    {
                        QuizId = q.QuizId,
                        Question = q.Question,
                        CorrectAnswer = q.CorrectAnswer,
                        IncorrectAnswers = q.IncorrectAnswers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Category = q.Category,
                        Difficulty = q.Difficulty,
                        CreatedAt = q.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedResponse<QuizResponse>
                {
                    Items = quizzes,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<QuizResponse>();
            }
        }


        // get quiz by id
        public async Task<QuizResponse?> GetByIdAsync(int id)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted);

                if (quiz is null) return null;

                return new QuizResponse
                {
                    QuizId = quiz.QuizId,
                    Question = quiz.Question,
                    CorrectAnswer = quiz.CorrectAnswer,
                    IncorrectAnswers = quiz.IncorrectAnswers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Category = quiz.Category,
                    Difficulty = quiz.Difficulty,
                    CreatedAt = quiz.CreatedAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // get random quizzes by category and difficulty
        public async Task<List<QuizResponse>> GetRandomAsync(string category, string difficulty, int count = 10)
        {
            try
            {
                return await _context.Quizzes
                    .AsNoTracking()
                    .Where(q => !q.IsDeleted && q.Category == category && q.Difficulty == difficulty)
                    .OrderBy(q => Guid.NewGuid())
                    .Take(count)
                    .Select(q => new QuizResponse
                    {
                        QuizId = q.QuizId,
                        Question = q.Question,
                        CorrectAnswer = q.CorrectAnswer,
                        IncorrectAnswers = q.IncorrectAnswers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Category = q.Category,
                        Difficulty = q.Difficulty,
                        CreatedAt = q.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<QuizResponse>();
            }
        }

        // get distinct categories
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                return await _context.Quizzes
                    .AsNoTracking()
                    .Where(q => !q.IsDeleted)
                    .Select(q => q.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        // create quiz
        public async Task<QuizResponse?> CreateAsync(CreateQuizRequest request)
        {
            try
            {
                var quiz = new Quiz
                {
                    Question = request.Question,
                    CorrectAnswer = request.CorrectAnswer,
                    IncorrectAnswers = string.Join(",", request.IncorrectAnswers),
                    Category = request.Category,
                    Difficulty = request.Difficulty,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Quizzes.Add(quiz);
                bool saved = await SaveAsync();

                if (!saved) return null;

                return new QuizResponse
                {
                    QuizId = quiz.QuizId,
                    Question = quiz.Question,
                    CorrectAnswer = quiz.CorrectAnswer,
                    IncorrectAnswers = request.IncorrectAnswers,
                    Category = quiz.Category,
                    Difficulty = quiz.Difficulty,
                    CreatedAt = quiz.CreatedAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // update quiz
        public async Task<QuizResponse?> UpdateAsync(int id, UpdateQuizRequest request)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted);

                if (quiz is null) return null;

                if (request.Question is not null) quiz.Question = request.Question;
                if (request.CorrectAnswer is not null) quiz.CorrectAnswer = request.CorrectAnswer;
                if (request.IncorrectAnswers is not null) quiz.IncorrectAnswers = string.Join(",", request.IncorrectAnswers);
                if (request.Category is not null) quiz.Category = request.Category;
                if (request.Difficulty is not null) quiz.Difficulty = request.Difficulty;

                bool saved = await SaveAsync();

                if (!saved) return null;

                return new QuizResponse
                {
                    QuizId = quiz.QuizId,
                    Question = quiz.Question,
                    CorrectAnswer = quiz.CorrectAnswer,
                    IncorrectAnswers = quiz.IncorrectAnswers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Category = quiz.Category,
                    Difficulty = quiz.Difficulty,
                    CreatedAt = quiz.CreatedAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // soft delete quiz
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted);

                if (quiz is null) return false;

                quiz.IsDeleted = true;
                quiz.DeletedAt = DateTime.UtcNow;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
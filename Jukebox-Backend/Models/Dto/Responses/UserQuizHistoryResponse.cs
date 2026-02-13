namespace Jukebox_Backend.Models.Dto.Responses
{
    public class UserQuizHistoryResponse
    {
        public int UserQuizId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int CoinsEarned { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
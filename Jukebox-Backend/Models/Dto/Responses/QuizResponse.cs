namespace Jukebox_Backend.Models.Dto.Responses
{
    public class QuizResponse
    {
        public int QuizId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public List<string> IncorrectAnswers { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
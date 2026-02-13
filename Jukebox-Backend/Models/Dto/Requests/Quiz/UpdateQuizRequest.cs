using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UpdateQuizRequest
    {
        [StringLength(200, ErrorMessage = "Question must be max 200 characters")]
        public string? Question { get; set; }

        [StringLength(100, ErrorMessage = "Correct answer must be max 100 characters")]
        public string? CorrectAnswer { get; set; }

        [MinLength(3, ErrorMessage = "Exactly 3 incorrect answers are required")]
        [MaxLength(3, ErrorMessage = "Exactly 3 incorrect answers are required")]
        public List<string>? IncorrectAnswers { get; set; }

        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; }

        [StringLength(20, ErrorMessage = "Difficulty must be max 20 characters")]
        [RegularExpression("^(Easy|Medium|Hard)$", ErrorMessage = "Difficulty must be Easy, Medium, or Hard")]
        public string? Difficulty { get; set; }
    }
}
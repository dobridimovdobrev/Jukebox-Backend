using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateQuizRequest
    {
        [Required(ErrorMessage = "Question is required")]
        [StringLength(200, ErrorMessage = "Question must be max 200 characters")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Correct answer is required")]
        [StringLength(100, ErrorMessage = "Correct answer must be max 100 characters")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incorrect answers are required")]
        [MinLength(3, ErrorMessage = "Exactly 3 incorrect answers are required")]
        [MaxLength(3, ErrorMessage = "Exactly 3 incorrect answers are required")]
        public List<string> IncorrectAnswers { get; set; } = new();

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Difficulty is required")]
        [StringLength(20, ErrorMessage = "Difficulty must be max 20 characters")]
        [RegularExpression("^(Easy|Medium|Hard)$", ErrorMessage = "Difficulty must be Easy, Medium, or Hard")]
        public string Difficulty { get; set; } = string.Empty;
    }
}
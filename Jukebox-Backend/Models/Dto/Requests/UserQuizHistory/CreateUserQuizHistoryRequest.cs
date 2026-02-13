using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateUserQuizHistoryRequest
    {
        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Difficulty is required")]
        [StringLength(20, ErrorMessage = "Difficulty must be max 20 characters")]
        [RegularExpression("^(Easy|Medium|Hard)$", ErrorMessage = "Difficulty must be Easy, Medium, or Hard")]
        public string Difficulty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Correct answers is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Correct answers must be >= 0")]
        public int CorrectAnswers { get; set; }

        [Required(ErrorMessage = "Wrong answers is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Wrong answers must be >= 0")]
        public int WrongAnswers { get; set; }

        [Required(ErrorMessage = "Coins earned is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Coins earned must be >= 0")]
        public int CoinsEarned { get; set; }
    }
}
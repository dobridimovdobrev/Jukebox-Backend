using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Jukebox_Backend.Models.Entities
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        [StringLength(200)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Required]
        // array string A,B,C
        public string IncorrectAnswers { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        // Easy, Medium, Hard
        public string Difficulty { get; set; } = string.Empty;

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

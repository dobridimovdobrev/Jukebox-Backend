using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class UserQuizHistory
    {
        [Key]
        public int UserQuizId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        // Music, general, in the future maybe i will add more categories

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        // easy, medium, hard, maybe expert later on, but i will decide in next months

        [StringLength(20)]
        public string Difficulty { get; set; } = string.Empty;

        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int CoinsEarned { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}

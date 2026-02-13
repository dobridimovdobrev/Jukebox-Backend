using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        public DateOnly Birthday { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Coins { get; set; } = 5;
        public int TotalSongsPlayed { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        //relations
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<UserQuizHistory> QuizHistory { get; set; } = new List<UserQuizHistory>();
    }
}

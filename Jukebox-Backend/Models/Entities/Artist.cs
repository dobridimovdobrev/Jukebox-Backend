using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Entities
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Photo { get; set; } = string.Empty;

        [StringLength(2)]
        public string? CountryCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Genre { get; set; } = string.Empty;

        public DateTime? CareerStart { get; set; }
        public DateTime? CareerEnd { get; set; }

        public int? SongsCount { get; set; } = 0;

        [MaxLength(100)]
        public string? YoutubeChannelId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MusicBrainzId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IsrcCode { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Biography { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Relation
        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}

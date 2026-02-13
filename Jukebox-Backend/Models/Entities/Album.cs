using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class Album
    {
        [Key]
        public int AlbumId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public int? YearReleased { get; set; }

        [StringLength(50)]
        public string? Genre { get; set; }

        [StringLength(50)]
        public string? Style { get; set; }

        [StringLength(50)]
        public string? Mood { get; set; }

        [StringLength(100)]
        public string? Label { get; set; }

        [StringLength(500)]
        public string? AlbumThumb { get; set; }

        [StringLength(5000)]
        public string? DescriptionEN { get; set; }

        [StringLength(100)]
        public string? MusicBrainzId { get; set; }

        [StringLength(100)]
        public string? TheAudioDbId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // foreign key
        [Required]
        public int ArtistId { get; set; }

        [ForeignKey(nameof(ArtistId))]
        public Artist? Artist { get; set; }

        // realòtion
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
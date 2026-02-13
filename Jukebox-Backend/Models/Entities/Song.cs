using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class Song
    {
        [Key]
        public int SongId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2)]
        public string? CountryCode { get; set; } = string.Empty;

        [Required]
        public int Duration { get; set; }

        public int? ReleaseYear { get; set; }


        [StringLength(50)]
        public string? Genre { get; set; } = string.Empty;

        // song played count
        public int SongsPlayed { get; set; } = 0;

        [Required]
        [StringLength(500)]
        public string YoutubeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MusicBrainzId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IsrcCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Foreign key
        [Required]
        public int ArtistId { get; set; }

        [ForeignKey(nameof(ArtistId))]
        public Artist? Artist { get; set; }

        // Album (optional - songs from theaudiodb have album, but music brainz may not)
        public int? AlbumId { get; set; }

        [ForeignKey(nameof(AlbumId))]
        public Album? Album { get; set; }

        // Relation
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}

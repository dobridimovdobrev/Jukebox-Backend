using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        /*[Required]*/
        [StringLength(50)]
        public string? Category { get; set; } = string.Empty;

        //30 , 50 ,70, 100
        public int? SongsCount { get; set; }

        public bool IsGenerated { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        // Relations 
        public ICollection<PlaylistArtist> PlaylistArtists { get; set; } = new List<PlaylistArtist>();
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}

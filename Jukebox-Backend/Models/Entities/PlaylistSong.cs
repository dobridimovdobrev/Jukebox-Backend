using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class PlaylistSong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PlaylistId { get; set; }

        [ForeignKey(nameof(PlaylistId))]
        public Playlist? Playlist { get; set; }

        [Required]
        public int SongId { get; set; }

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(SongId))]
        public Song? Song { get; set; }

        public int Order { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}

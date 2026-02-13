using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class PlaylistArtist
    {
        [Key]
        public int Id { get; set; }

        public int CareerStart { get; set; }
        public int CareerEnd { get; set; }

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }


        public int PlaylistId { get; set; }
        [ForeignKey(nameof(PlaylistId))]
        public Playlist? Playlist { get; set; }


        public int ArtistId { get; set; }
        [ForeignKey(nameof(ArtistId))]
        public Artist? Artist { get; set; }
    }
}

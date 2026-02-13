using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateArtistRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be max 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Photo is required")]
        [StringLength(500, ErrorMessage = "Photo URL must be max 500 characters")]
        public string Photo { get; set; } = string.Empty;

        [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string? CountryCode { get; set; }

        [StringLength(50, ErrorMessage = "Genre must be max 50 characters")]
        public string? Genre { get; set; }

        public DateTime? CareerStart { get; set; }
        public DateTime? CareerEnd { get; set; }

        [MaxLength(100, ErrorMessage = "YouTube Channel ID must be max 100 characters")]
        public string? YoutubeChannelId { get; set; }

        [MaxLength(100, ErrorMessage = "MusicBrainz ID must be max 100 characters")]
        public string? MusicBrainzId { get; set; }

        [MaxLength(100, ErrorMessage = "ISRC Code must be max 100 characters")]
        public string? IsrcCode { get; set; }

        [StringLength(1000, ErrorMessage = "Biography must be max 1000 characters")]
        public string? Biography { get; set; }
    }
}
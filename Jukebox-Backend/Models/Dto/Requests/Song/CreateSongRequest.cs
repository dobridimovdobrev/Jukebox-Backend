using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateSongRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title must be max 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string? CountryCode { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Release year is required")]
        [Range(1900, 2100, ErrorMessage = "Release year must be between 1900 and 2100")]
        public int ReleaseYear { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre must be max 50 characters")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "YouTube ID is required")]
        [StringLength(500, ErrorMessage = "YouTube ID must be max 500 characters")]
        public string YoutubeId { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "MusicBrainz ID must be max 100 characters")]
        public string? MusicBrainzId { get; set; }

        [MaxLength(100, ErrorMessage = "ISRC Code must be max 100 characters")]
        public string? IsrcCode { get; set; }

        [Required(ErrorMessage = "Artist ID is required")]
        public int ArtistId { get; set; }
    }
}

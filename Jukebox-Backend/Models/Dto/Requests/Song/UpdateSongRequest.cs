using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UpdateSongRequest
    {
        [StringLength(100, ErrorMessage = "Title must be max 100 characters")]
        public string? Title { get; set; }

        [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string? CountryCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int? Duration { get; set; }

        [Range(1900, 2100, ErrorMessage = "Release year must be between 1900 and 2100")]
        public int? ReleaseYear { get; set; }

        [StringLength(50, ErrorMessage = "Genre must be max 50 characters")]
        public string? Genre { get; set; }

        [StringLength(500, ErrorMessage = "YouTube ID must be max 500 characters")]
        public string? YoutubeId { get; set; }

        [MaxLength(100, ErrorMessage = "MusicBrainz ID must be max 100 characters")]
        public string? MusicBrainzId { get; set; }

        [MaxLength(100, ErrorMessage = "ISRC Code must be max 100 characters")]
        public string? IsrcCode { get; set; }

        public int? ArtistId { get; set; }
    }
}
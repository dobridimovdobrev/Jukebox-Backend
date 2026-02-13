using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class SearchSongRequest
    {
        [StringLength(100, ErrorMessage = "Title must be max 100 characters")]
        public string? Title { get; set; }

        public int? ArtistId { get; set; }

        [StringLength(50, ErrorMessage = "Genre must be max 50 characters")]
        public string? Genre { get; set; }

        public int? ReleaseYearFrom { get; set; }
        public int? ReleaseYearTo { get; set; }

        [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string? CountryCode { get; set; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 30;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be >= 1")]
        public int PageNumber { get; set; } = 1;
    }
}
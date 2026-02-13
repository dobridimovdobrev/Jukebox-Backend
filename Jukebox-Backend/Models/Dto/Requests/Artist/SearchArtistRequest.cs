using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class SearchArtistRequest
    {
        [StringLength(100, ErrorMessage = "Search term must be max 100 characters")]
        public string? Name { get; set; }

        [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
        public string? CountryCode { get; set; }

        [StringLength(50, ErrorMessage = "Genre must be max 50 characters")]
        public string? Genre { get; set; }

        public int? CareerStartYear { get; set; }
        public int? CareerEndYear { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 30;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be >= 1")]
        public int PageNumber { get; set; } = 1;
    }
}
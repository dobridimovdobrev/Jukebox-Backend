using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class SearchTicketRequest
    {
        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Range(1, 100)]
        public int PageSize { get; set; } = 30;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;
    }
}
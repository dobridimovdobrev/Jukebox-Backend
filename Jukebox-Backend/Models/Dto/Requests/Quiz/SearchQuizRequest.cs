using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class SearchQuizRequest
    {
        [StringLength(200)]
        public string? Question { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(20)]
        public string? Difficulty { get; set; }

        [Range(1, 100)]
        public int PageSize { get; set; } = 30;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;
    }
}
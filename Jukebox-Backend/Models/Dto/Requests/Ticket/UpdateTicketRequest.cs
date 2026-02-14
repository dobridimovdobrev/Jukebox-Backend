using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UpdateTicketRequest
    {
        [StringLength(100, ErrorMessage = "Title must be max 100 characters")]
        public string? Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be max 2000 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Status must be max 50 characters")]
        [RegularExpression("^(open|in_progress|answered|closed)$", ErrorMessage = "Status must be open, in_progress, answered, or closed")]
        public string? Status { get; set; }

        [StringLength(20, ErrorMessage = "Priority must be max 20 characters")]
        [RegularExpression("^(Low|Medium|High)$", ErrorMessage = "Priority must be Low, Medium, or High")]
        public string? Priority { get; set; }

        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; }

        public string? AttachmentUrl { get; set; }
    }
}
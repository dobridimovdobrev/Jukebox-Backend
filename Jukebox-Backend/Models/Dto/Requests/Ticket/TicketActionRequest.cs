using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class TicketActionRequest
    {
        [StringLength(2000, ErrorMessage = "Message must be max 2000 characters")]
        public string? Message { get; set; }

        public string? AttachmentUrl { get; set; }

        [StringLength(50)]
        [RegularExpression("^(open|in_progress|answered|closed)$",
            ErrorMessage = "Status must be open, in_progress, answered, or closed")]
        public string? NewStatus { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
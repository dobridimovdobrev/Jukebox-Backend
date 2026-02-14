using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UserReplyRequest
    {
        [StringLength(2000, ErrorMessage = "Message must be max 2000 characters")]
        public string? Message { get; set; }

        public string? AttachmentUrl { get; set; }
    }
}
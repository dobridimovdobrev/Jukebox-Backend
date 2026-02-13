using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateTicketRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title must be max 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description must be max 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(20, ErrorMessage = "Priority must be max 20 characters")]
        [RegularExpression("^(Low|Medium|High)$", ErrorMessage = "Priority must be Low, Medium, or High")]
        public string Priority { get; set; } = "Low";

        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; }

        public string? AttachmentUrl { get; set; }
    }
}

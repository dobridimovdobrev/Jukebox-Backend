using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        // Open, In Progress, Resolved, Closed
        public string Status { get; set; } = "open";

        [Required]
        [StringLength(20)]
        // Low, Medium, High
        public string Priority { get; set; } = "low";

        [Required]
        [StringLength(50)]
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public string? AttachmentUrl { get; set; }

        //soft delete ( i don't want to delete data for real)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }


        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]

        public ApplicationUser? User { get; set; }
    }
}

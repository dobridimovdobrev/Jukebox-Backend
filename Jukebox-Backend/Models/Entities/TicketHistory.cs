using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jukebox_Backend.Models.Entities
{
    public class TicketHistory
    {
        [Key]
        public int TicketHistoryId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey(nameof(TicketId))]
        public Ticket? Ticket { get; set; }

        [Required]
        [StringLength(20)]

        public string Type { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FromStatus { get; set; }

        [StringLength(50)]
        public string? ToStatus { get; set; }

        [StringLength(2000)]
        public string? Message { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public string? AttachmentUrl { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
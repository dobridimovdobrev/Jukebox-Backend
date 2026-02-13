namespace Jukebox_Backend.Models.Dto.Responses
{
    public class TicketResponse
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AttachmentUrl { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public List<TicketHistoryResponse> History { get; set; } = new();
    }

    public class TicketHistoryResponse
    {
        public int TicketHistoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? Message { get; set; }
        public string? Note { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? By { get; set; }
        public DateTime Date { get; set; }
    }
}
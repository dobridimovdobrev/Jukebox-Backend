namespace Jukebox_Backend.Models.Dto.Responses
{
    public class UserResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateOnly Birthday { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Coins { get; set; }
        public int TotalSongsPlayed { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
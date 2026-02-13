namespace Jukebox_Backend.Models.Dto.Responses
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

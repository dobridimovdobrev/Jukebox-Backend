using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UpdateUserRequest
    {
        [StringLength(50, ErrorMessage = "First name must be max 50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name must be max 50 characters")]
        public string? LastName { get; set; }

        [StringLength(10, ErrorMessage = "Gender must be max 10 characters")]
        public string? Gender { get; set; }

        [StringLength(50, ErrorMessage = "Country must be max 50 characters")]
        public string? Country { get; set; }

        public DateOnly? Birthday { get; set; }
    }
}
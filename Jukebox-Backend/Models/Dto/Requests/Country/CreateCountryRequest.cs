using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreateCountryRequest
    {
        [Required(ErrorMessage = "Code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Code must be exactly 2 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name must be max 50 characters")]
        public string Name { get; set; } = string.Empty;
    }
}
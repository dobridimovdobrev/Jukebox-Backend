using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class CreatePlaylistRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be max 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must be max 500 characters")]
        public string? Description { get; set; }

        /*[Required(ErrorMessage = "Category is required")]*/
        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; } = string.Empty;

        [Range(30, 100, ErrorMessage = "Songs count must be 30, 50, 70, or 100")]
        public int? SongsCount { get; set; }
    }
}

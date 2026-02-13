using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class UpdatePlaylistRequest
    {
        [StringLength(100, ErrorMessage = "Name must be max 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must be max 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; }
    }
}
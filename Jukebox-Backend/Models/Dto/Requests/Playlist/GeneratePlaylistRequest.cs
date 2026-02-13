using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class GeneratePlaylistRequest
    {
        [Required(ErrorMessage = "Playlist name is required")]
        [StringLength(100, ErrorMessage = "Name must be max 100 characters")]
        public string PlaylistName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must be max 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Category must be max 50 characters")]
        public string? Category { get; set; } = string.Empty;

        // 30 songs per artist, fixed in backend i will change this in future now i should finish the project
        [Required(ErrorMessage = "At least one artist is required")]
        [MinLength(1, ErrorMessage = "At least 1 artist is required")]
        [MaxLength(5, ErrorMessage = "Maximum 5 artists allowed")]
        public List<PlaylistArtistSelection> Artists { get; set; } = new();
    }

    public class PlaylistArtistSelection
    {
        [Required(ErrorMessage = "Artist ID is required")]
        public int ArtistId { get; set; }
    }
}
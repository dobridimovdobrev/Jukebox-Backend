using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Dto.Requests
{
    public class ImportMusicBrainzRequest
    {
        [Required]
        public string MusicBrainzId { get; set; } = string.Empty;
    }
}
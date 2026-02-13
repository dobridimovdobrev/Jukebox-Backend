namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class MusicBrainzSongResult
    {
        public string MusicBrainzId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? Duration { get; set; }
        public int? ReleaseYear { get; set; }
        public string? IsrcCode { get; set; }
    }
}

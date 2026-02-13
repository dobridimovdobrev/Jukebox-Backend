namespace Jukebox_Backend.Models.Dto.Responses
{
    public class ArtistResponse
    {
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? Genre { get; set; }
        public int? CareerStart { get; set; }
        public int? CareerEnd { get; set; }
        public int? SongsCount { get; set; }
        public string? YoutubeChannelId { get; set; }
        public string? MusicBrainzId { get; set; }
        public string? IsrcCode { get; set; }
        public string? Biography { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
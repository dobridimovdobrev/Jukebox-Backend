namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class YoutubeResult
    {
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ChannelTitle { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
    }
}

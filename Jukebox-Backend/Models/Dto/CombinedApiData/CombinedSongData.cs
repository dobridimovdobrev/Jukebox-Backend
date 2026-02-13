namespace Jukebox_Backend.Models.Dto.CombinedApiData
{
    // i need this for joininig data  from 2 different api's music brainz + youtube
    public class CombinedSongData
    {
        public string Title { get; set; } = string.Empty;
        public string MusicBrainzId { get; set; } = string.Empty;
        public string? IsrcCode { get; set; }
        public int? Duration { get; set; }
        public int? ReleaseYear { get; set; }
        public string? YoutubeId { get; set; }
        public int ArtistId { get; set; }
    }
}

namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class MusicBrainzArtistResult
    {
        public string MusicBrainzId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public DateTime? CareerStart { get; set; }
        public DateTime? CareerEnd { get; set; }
        public int? Score { get; set; }
    }
}

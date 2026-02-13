namespace Jukebox_Backend.Models.Dto.CombinedApiData
{

    // i need this for joininig data  from 2 different api's musicbrainz + theaudiodb(photo and bio)
    public class CombinedArtistData
    {
        public string Name { get; set; } = string.Empty;
        public string MusicBrainzId { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public DateTime? CareerStart { get; set; }
        public DateTime? CareerEnd { get; set; }
        public string? Photo { get; set; }
        public string? Biography { get; set; }
        public string? Genre { get; set; }
        public bool IsActive => CareerEnd == null;
    }
}

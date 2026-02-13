namespace Jukebox_Backend.Models.Dto.Responses
{
    public class SongResponse
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public int Duration { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Genre { get; set; } = string.Empty;
        public int SongsPlayed { get; set; }
        public string YoutubeId { get; set; } = string.Empty;
        public string? MusicBrainzId { get; set; }
        public string? IsrcCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public string? ArtistName { get; set; }
    }
}
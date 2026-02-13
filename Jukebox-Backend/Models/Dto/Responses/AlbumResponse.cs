namespace Jukebox_Backend.Models.Dto.Responses
{
    public class AlbumResponse
    {
        public int AlbumId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? YearReleased { get; set; }
        public string? Genre { get; set; }
        public string? Style { get; set; }
        public string? Mood { get; set; }
        public string? Label { get; set; }
        public string? AlbumThumb { get; set; }
        public string? DescriptionEN { get; set; }
        public int ArtistId { get; set; }
        public string? ArtistName { get; set; }
        public int SongsCount { get; set; }
    }
}
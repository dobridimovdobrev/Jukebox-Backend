namespace Jukebox_Backend.Models.Dto.Responses
{
    public class PlaylistResponse
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; } = string.Empty;
        public int? SongsCount { get; set; }
        public bool IsGenerated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public List<PlaylistArtistInfo>? Artists { get; set; }
        public List<PlaylistSongInfo>? Songs { get; set; }
    }

    public class PlaylistArtistInfo
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public int? CareerStart { get; set; }
        public int? CareerEnd { get; set; }
    }

    public class PlaylistSongInfo
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string YoutubeId { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
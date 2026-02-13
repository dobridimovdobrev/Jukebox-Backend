using System.Text.Json.Serialization;

namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class TheAudioDbTrackResult
    {
        [JsonPropertyName("idTrack")]
        public string? IdTrack { get; set; }

        [JsonPropertyName("idAlbum")]
        public string? IdAlbum { get; set; }

        [JsonPropertyName("idArtist")]
        public string? IdArtist { get; set; }

        [JsonPropertyName("strTrack")]
        public string? StrTrack { get; set; }

        [JsonPropertyName("strAlbum")]
        public string? StrAlbum { get; set; }

        [JsonPropertyName("strArtist")]
        public string? StrArtist { get; set; }

        [JsonPropertyName("intDuration")]
        public string? IntDuration { get; set; }

        [JsonPropertyName("intTrackNumber")]
        public string? IntTrackNumber { get; set; }

        [JsonPropertyName("strGenre")]
        public string? StrGenre { get; set; }

        [JsonPropertyName("strMusicVid")]
        public string? StrMusicVid { get; set; }

        [JsonPropertyName("strMusicBrainzID")]
        public string? StrMusicBrainzID { get; set; }
    }
}
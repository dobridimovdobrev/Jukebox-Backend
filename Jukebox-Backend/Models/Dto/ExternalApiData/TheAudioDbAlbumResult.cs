using System.Text.Json.Serialization;

namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class TheAudioDbAlbumResult
    {
        [JsonPropertyName("idAlbum")]
        public string? IdAlbum { get; set; }

        [JsonPropertyName("idArtist")]
        public string? IdArtist { get; set; }

        [JsonPropertyName("strAlbum")]
        public string? StrAlbum { get; set; }

        [JsonPropertyName("strArtist")]
        public string? StrArtist { get; set; }

        [JsonPropertyName("intYearReleased")]
        public string? IntYearReleased { get; set; }

        [JsonPropertyName("strStyle")]
        public string? StrStyle { get; set; }

        [JsonPropertyName("strGenre")]
        public string? StrGenre { get; set; }

        [JsonPropertyName("strMood")]
        public string? StrMood { get; set; }

        [JsonPropertyName("strLabel")]
        public string? StrLabel { get; set; }

        [JsonPropertyName("strAlbumThumb")]
        public string? StrAlbumThumb { get; set; }

        [JsonPropertyName("strDescriptionEN")]
        public string? StrDescriptionEN { get; set; }

        [JsonPropertyName("strMusicBrainzID")]
        public string? StrMusicBrainzID { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace Jukebox_Backend.Models.Dto.ExternalApiData
{
    public class TheAudioDbArtistResult
    {
        [JsonPropertyName("idArtist")]
        public string? IdArtist { get; set; }

        [JsonPropertyName("strArtist")]
        public string StrArtist { get; set; } = string.Empty;

        [JsonPropertyName("strArtistThumb")]
        public string? StrArtistThumb { get; set; }

        [JsonPropertyName("strBiographyEN")]
        public string? StrBiographyEN { get; set; }

        [JsonPropertyName("strGenre")]
        public string? StrGenre { get; set; }

        [JsonPropertyName("strStyle")]
        public string? StrStyle { get; set; }

        [JsonPropertyName("strMood")]
        public string? StrMood { get; set; }

        [JsonPropertyName("strCountryCode")]
        public string? StrCountryCode { get; set; }

        [JsonPropertyName("strCountry")]
        public string? StrCountry { get; set; }

        [JsonPropertyName("strLabel")]
        public string? StrLabel { get; set; }

        [JsonPropertyName("strWebsite")]
        public string? StrWebsite { get; set; }

        [JsonPropertyName("strMusicBrainzID")]
        public string? StrMusicBrainzID { get; set; }

        [JsonPropertyName("intFormedYear")]
        public string? IntFormedYear { get; set; }

        [JsonPropertyName("intDiedYear")]
        public string? IntDiedYear { get; set; }
    }
}
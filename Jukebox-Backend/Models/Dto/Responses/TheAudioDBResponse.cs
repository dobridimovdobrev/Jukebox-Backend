using System.Text.Json.Serialization;

namespace Jukebox_Backend.Models.Dto.Responses
{
    public class TheAudioDBResponse
    {
        [JsonPropertyName("artists")]
        public List<TheAudioDBArtist>? Artists { get; set; }
    }

    public class TheAudioDBArtist
    {
        [JsonPropertyName("strArtist")]
        public string StrArtist { get; set; } = string.Empty;

        [JsonPropertyName("strArtistThumb")]
        public string? StrArtistThumb { get; set; }

        [JsonPropertyName("strBiographyEN")]
        public string? StrBiographyEN { get; set; }

        [JsonPropertyName("strGenre")]
        public string? StrGenre { get; set; }
    }
}
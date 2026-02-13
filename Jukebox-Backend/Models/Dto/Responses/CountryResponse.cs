namespace Jukebox_Backend.Models.Dto.Responses
{
    public class CountryResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace Jukebox_Backend.Models.Entities
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}

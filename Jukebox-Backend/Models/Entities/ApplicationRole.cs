using Microsoft.AspNetCore.Identity;

namespace Jukebox_Backend.Models.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public bool Active { get; set; }
    }
}

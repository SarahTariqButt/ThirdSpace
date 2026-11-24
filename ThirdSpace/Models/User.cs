using Microsoft.AspNetCore.Identity;

namespace ThirdSpace.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePicture { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}

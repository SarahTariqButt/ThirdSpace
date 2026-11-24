
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Home
{
    public class Story
    {
        public int Id { get; set; }
        public string MediaPath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }
            = DateTime.UtcNow.AddHours(24);
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}

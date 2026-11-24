
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Home
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}

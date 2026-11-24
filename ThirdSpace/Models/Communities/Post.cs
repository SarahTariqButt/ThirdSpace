using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Communities
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CommunityId { get; set; }

        public Community Community { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}


using System.ComponentModel.DataAnnotations;

namespace ThirdSpace.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}

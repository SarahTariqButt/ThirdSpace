
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Marketplace
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        [Required]
        public string SenderId { get; set; } = string.Empty;
        public User Sender { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

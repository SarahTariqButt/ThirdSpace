
using ThirdSpace.Models;

namespace ThirdSpace.Models.Marketplace
{
    public class Chat
    {
        public int Id { get; set; }

        public int ListingId { get; set; }

        public Listing Listing { get; set; } = null!;

        public string BuyerId { get; set; } = string.Empty;

        public User Buyer { get; set; } = null!;

        public string SellerId { get; set; } = string.Empty;

        public User Seller { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; }
            = new List<ChatMessage>();
    }
}

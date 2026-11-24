
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Marketplace
{
    public class Listing
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool InStock { get; set; } = true;

        public string SellerId { get; set; } = string.Empty;

        public User Seller { get; set; } = null!;
    }
}


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ThirdSpace.ViewModels
{
    public class MarketplaceListingInput
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999.99")]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Condition { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public bool InStock { get; set; } = true;

        public IFormFile? Photo { get; set; }

        public string? ExistingImagePath { get; set; }
    }
}

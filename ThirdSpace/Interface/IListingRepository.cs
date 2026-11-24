
using ThirdSpace.Models.Marketplace;

namespace ThirdSpace.Interfaces
{
    public interface IListingRepository
    {
        Task<List<Listing>> GetAllAsync();

        Task<List<Listing>> SearchAsync(
            string? query,
            string? category,
            decimal? minPrice,
            decimal? maxPrice);

        Task<List<Listing>> GetBySellerAsync(string sellerId);

        Task<Listing?> GetByIdAsync(int id);

        Task AddAsync(Listing listing);

        Task UpdateAsync(Listing listing);
    }
}

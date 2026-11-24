
using Dapper;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Interfaces;
using ThirdSpace.Models.Marketplace;

namespace ThirdSpace.Repositories
{
    public class ListingRepository : IListingRepository
    {
        private readonly ApplicationDbContext _context;

        public ListingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // DAPPER: Get available listings
        public async Task<List<Listing>> GetAllAsync()
        {
            var connection = _context.Database.GetDbConnection();

            string sql = @"
                SELECT *
                FROM [Listings]
                WHERE [InStock] = 1
                ORDER BY [CreatedAt] DESC";

            var listings = (await connection
                .QueryAsync<Listing>(sql))
                .ToList();

            await LoadSellersAsync(listings);

            return listings;
        }

        public async Task<List<Listing>> SearchAsync(
            string? query,
            string? category,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var connection = _context.Database.GetDbConnection();

            string sql = @"
                SELECT *
                FROM [Listings]
                WHERE [InStock] = 1
                  AND (
                      @Query IS NULL
                      OR [Title] LIKE @SearchPattern
                      OR [Description] LIKE @SearchPattern
                      OR [Location] LIKE @SearchPattern
                  )
                  AND (
                      @Category IS NULL
                      OR [Category] = @Category
                  )
                  AND (
                      @MinPrice IS NULL
                      OR [Price] >= @MinPrice
                  )
                  AND (
                      @MaxPrice IS NULL
                      OR [Price] <= @MaxPrice
                  )
                ORDER BY [CreatedAt] DESC";

            var searchTerm = string.IsNullOrWhiteSpace(query)
                ? null
                : query.Trim();

            var selectedCategory = string.IsNullOrWhiteSpace(category)
                ? null
                : category.Trim();

            string? searchPattern = searchTerm == null
                ? null
                : "%" + searchTerm
                    .Replace("[", "[[]")
                    .Replace("%", "[%]")
                    .Replace("_", "[_]") + "%";

            var parameters = new
            {
                Query = searchTerm,
                SearchPattern = searchPattern,
                Category = selectedCategory,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var listings = (await connection
                .QueryAsync<Listing>(sql, parameters))
                .ToList();

            await LoadSellersAsync(listings);

            return listings;
        }

        private async Task LoadSellersAsync(List<Listing> listings)
        {
            var sellerIds = listings
                .Select(l => l.SellerId)
                .Distinct()
                .ToList();

            if (sellerIds.Count == 0)
                return;

            var sellers = await _context.Users
                .AsNoTracking()
                .Where(u => sellerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var listing in listings)
            {
                if (sellers.TryGetValue(
                    listing.SellerId,
                    out var seller))
                {
                    listing.Seller = seller;
                }
            }
        }

        public async Task<List<Listing>> GetBySellerAsync(
            string sellerId)
        {
            return await _context.Listings
                .Where(l => l.SellerId == sellerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Listing?> GetByIdAsync(int id)
        {
            return await _context.Listings
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(Listing listing)
        {
            await _context.Listings.AddAsync(listing);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Listing listing)
        {
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
        }
    }
}

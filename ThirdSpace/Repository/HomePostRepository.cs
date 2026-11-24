
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Interfaces;
using ThirdSpace.Models.Home;

namespace ThirdSpace.Repositories
{
    public class HomePostRepository : IHomePostRepository
    {
        private readonly ApplicationDbContext _context;

        public HomePostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllAsync()
        {
            return await _context.HomePosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetByUserAsync(string userId)
        {
            return await _context.HomePosts
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.HomePosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Post post)
        {
            await _context.HomePosts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            _context.HomePosts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _context.HomePosts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}

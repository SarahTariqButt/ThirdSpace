
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Interfaces;
using ThirdSpace.Models.Communities;

namespace ThirdSpace.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Community>> GetAllAsync()
        {
            return await _context.Communities
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Community?> GetByIdAsync(int id)
        {
            return await _context.Communities
                .Include(c => c.Creator)
                .Include(c => c.Members)
                .Include(c => c.Posts)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Community>> GetJoinedCommunitiesAsync(string userId)
        {
            return await _context.CommunityMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.Community)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Community>> GetPopularCommunitiesAsync(int count = 5)
        {
            return await _context.Communities
                .OrderByDescending(c => c.Members.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(int communityId, string userId)
        {
            return await _context.CommunityMembers
                .AnyAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);
        }

        public async Task AddAsync(Community community)
        {
            await _context.Communities.AddAsync(community);
            await _context.SaveChangesAsync();
        }

        public async Task AddMemberAsync(CommunityMember member)
        {
            await _context.CommunityMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(CommunityMember member)
        {
            _context.CommunityMembers.Remove(member);
            await _context.SaveChangesAsync();
        }

        public async Task<CommunityMember?> GetMemberAsync(
            int communityId,
            string userId)
        {
            return await _context.CommunityMembers
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Communities
                .AnyAsync(c => c.Name == name);
        }
    }
}

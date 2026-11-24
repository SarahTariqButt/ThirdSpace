
using ThirdSpace.Models.Communities;

namespace ThirdSpace.Interfaces
{
    public interface ICommunityRepository
    {
        Task<List<Community>> GetAllAsync();

        Task<Community?> GetByIdAsync(int id);

        Task<List<Community>> GetJoinedCommunitiesAsync(string userId);

        Task<List<Community>> GetPopularCommunitiesAsync(int count = 5);

        Task<bool> IsMemberAsync(int communityId, string userId);

        Task AddAsync(Community community);

        Task AddMemberAsync(CommunityMember member);

        Task RemoveMemberAsync(CommunityMember member);

        Task<CommunityMember?> GetMemberAsync(int communityId, string userId);

        Task<bool> NameExistsAsync(string name);
    }
}

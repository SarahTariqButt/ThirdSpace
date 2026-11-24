
using ThirdSpace.Models.Home;

namespace ThirdSpace.Interfaces
{
    public interface IHomePostRepository
    {
        Task<List<Post>> GetAllAsync();

        Task<List<Post>> GetByUserAsync(string userId);

        Task<Post?> GetByIdAsync(int id);

        Task AddAsync(Post post);

        Task UpdateAsync(Post post);

        Task DeleteAsync(Post post);
    }
}

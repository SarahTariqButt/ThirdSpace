
using ThirdSpace.Models.Marketplace;

namespace ThirdSpace.Interfaces
{
    public interface IChatRepository
    {
        Task<List<Chat>> GetUserChatsAsync(string userId);

        Task<Chat?> GetByIdAsync(int chatId, string userId);

        Task<Chat?> FindChatAsync(
            int listingId,
            string buyerId,
            string sellerId);

        Task<Chat> AddChatAsync(Chat chat);

        Task<List<ChatMessage>> GetMessagesAsync(int chatId);

        Task AddMessageAsync(ChatMessage message);
    }
}

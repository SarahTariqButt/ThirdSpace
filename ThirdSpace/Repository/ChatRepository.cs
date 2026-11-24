
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Interfaces;
using ThirdSpace.Models.Marketplace;

namespace ThirdSpace.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _context.Chats
                .AsNoTracking()
                .Include(c => c.Listing)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Where(c =>
                    c.BuyerId == userId ||
                    c.SellerId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Chat?> GetByIdAsync(int chatId, string userId)
        {
            return await _context.Chats
                .AsNoTracking()
                .Include(c => c.Listing)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c =>
                    c.Id == chatId &&
                    (c.BuyerId == userId ||
                     c.SellerId == userId));
        }

        public async Task<Chat?> FindChatAsync(
            int listingId,
            string buyerId,
            string sellerId)
        {
            return await _context.Chats
                .FirstOrDefaultAsync(c =>
                    c.ListingId == listingId &&
                    c.BuyerId == buyerId &&
                    c.SellerId == sellerId);
        }

        public async Task<Chat> AddChatAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(int chatId)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }
    }
}

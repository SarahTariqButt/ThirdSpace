
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Models;

namespace ThirdSpace.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ThirdSpace.Models.Home.Post> HomePosts { get; set; }
        public DbSet<ThirdSpace.Models.Home.Story> Stories { get; set; }

        public DbSet<ThirdSpace.Models.Communities.Community> Communities { get; set; }
        public DbSet<ThirdSpace.Models.Communities.CommunityMember> CommunityMembers { get; set; }
        public DbSet<ThirdSpace.Models.Communities.Post> CommunityPosts { get; set; }

        public DbSet<ThirdSpace.Models.Marketplace.Listing> Listings { get; set; }
        public DbSet<ThirdSpace.Models.Marketplace.Chat> Chats { get; set; }
        public DbSet<ThirdSpace.Models.Marketplace.ChatMessage> ChatMessages { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ThirdSpace.Models.Marketplace.Listing>()
    .Property(l => l.Price)
    .HasPrecision(18, 2);
            builder.Entity<FriendRequest>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FriendRequest>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.Chat>()
                .HasOne(c => c.Buyer)
                .WithMany()
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.Chat>()
                .HasOne(c => c.Seller)
                .WithMany()
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.Chat>()
                .HasOne(c => c.Listing)
                .WithMany()
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.CommunityMember>()
                .HasIndex(m => new { m.UserId, m.CommunityId })
                .IsUnique();

            builder.Entity<ThirdSpace.Models.Communities.Community>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<ThirdSpace.Models.Home.Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Home.Story>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.Post>()
                .HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.Community>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.CommunityMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Communities.CommunityMember>()
                .HasOne(m => m.Community)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.CommunityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.Listing>()
                .HasOne(l => l.Seller)
                .WithMany()
                .HasForeignKey(l => l.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ThirdSpace.Models.Marketplace.ChatMessage>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}

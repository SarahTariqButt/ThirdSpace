using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Models;

namespace ThirdSpace.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

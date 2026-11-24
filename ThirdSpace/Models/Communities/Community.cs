
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Communities
{
    public class Community
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatorId { get; set; } = string.Empty;

        public User Creator { get; set; } = null!;
        public ICollection<CommunityMember> Members { get; set; }
            = new List<CommunityMember>();
        public ICollection<Post> Posts { get; set; }
            = new List<Post>();
    }
}

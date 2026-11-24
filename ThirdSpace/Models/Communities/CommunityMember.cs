
using System.ComponentModel.DataAnnotations;
using ThirdSpace.Models;

namespace ThirdSpace.Models.Communities
{
    public class CommunityMember
    {
        public int Id { get; set; }

        public int CommunityId { get; set; }

        public Community Community { get; set; } = null!;

        
        public string UserId { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}


using Microsoft.AspNetCore.Identity;

namespace ThirdSpace.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Post> Posts { get; set; }
            = new List<Post>();
    }
}

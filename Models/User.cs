using Microsoft.AspNetCore.Identity;

namespace SocialMediaAPI.Models
{
    public class ApiUser : IdentityUser
    {
        public ICollection<Post>? Posts { get; set; }
        public ICollection<ApiUser>? Followers { get; set; }
    }
}

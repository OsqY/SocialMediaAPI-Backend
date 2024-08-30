using Microsoft.AspNetCore.Identity;

namespace SocialMediaAPI.Models
{
    public class ApiUser : IdentityUser
    {
        public ICollection<Post>? Posts { get; set; }
        public ICollection<Post>? LikedPosts { get; set; }
        public ICollection<ApiUser>? Followers { get; set; }
        public ICollection<ApiUser>? Following { get; set; }
        public ICollection<ChatMessage>? SentMessages { get; set; }
        public ICollection<ChatMessage>? ReceivedMessages { get; set; }
    }
}

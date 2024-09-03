using Microsoft.AspNetCore.Identity;

namespace SocialMediaAPI.Models
{
    public class ApiUser : IdentityUser
    {
        public ApiUser()
        {
            Posts = new List<Post>();
            LikedPosts = new List<Post>();
            Followers = new List<ApiUser>();
            Following = new List<ApiUser>();
            SentMessages = new List<ChatMessage>();
            ReceivedMessages = new List<ChatMessage>();
            Comments = new List<Comment>();
        }

        public ICollection<Post>? Posts { get; set; }
        public ICollection<Post>? LikedPosts { get; set; }
        public ICollection<ApiUser>? Followers { get; set; }
        public ICollection<ApiUser>? Following { get; set; }
        public ICollection<ChatMessage>? SentMessages { get; set; }
        public ICollection<ChatMessage>? ReceivedMessages { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaAPI.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApiUser? User { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        public ICollection<Comment>? Comments { get; set; }

        public ICollection<ApiUser>? LikedByUsers { get; set; }
    }
}

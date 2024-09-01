using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaAPI.Models
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApiUser? User { get; set; }

        [Required]
        public int PostId { get; set; }
        public Post? Post { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int Likes { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        public ICollection<Comment>? Comments { get; set; }
    }
}

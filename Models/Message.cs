using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    public string Content { get; set; }

    [Required]
    public string SenderId { get; set; }

    [Required]
    public string ReceiverId { get; set; }

    public ApiUser? Sender { get; set; }
    public ApiUser? Receiver { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastModifiedDate { get; set; }
}

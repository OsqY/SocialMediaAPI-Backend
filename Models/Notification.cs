using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Models;

public class Notification
{
    [Key]
    [Required]
    public int Id { get; set; }

    public string Content { get; set; }

    public DateTime CreatedDage { get; set; }
}

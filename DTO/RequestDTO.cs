using System.ComponentModel;

namespace SocialMediaAPI.DTO;

public class RequestDTO<T>
{
    [DefaultValue("")]
    public string? Username { get; set; } = "";
}

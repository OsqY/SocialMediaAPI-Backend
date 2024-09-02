namespace SocialMediaAPI.DTO;

public class UserDTO
{
    public string? Id { get; set; }
    public string? Email { get; set; } = string.Empty;
    public string? Username { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public bool? IsCurrentUserFollowing { get; set; }
}

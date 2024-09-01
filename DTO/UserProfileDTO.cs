namespace SocialMediaAPI.DTO;

public class UserProfileDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public List<PostDTO> Posts { get; set; }
    public bool IsOwnUserProfile { get; set; } = false;
}

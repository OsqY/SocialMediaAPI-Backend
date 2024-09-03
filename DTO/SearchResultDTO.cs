namespace SocialMediaAPI.DTO;

public class SearchResultDTO
{
    public TYPE_OF_RESULT ResultType { get; set; }
    public string? Username { get; set; }
    public List<PostDTO>? Posts { get; set; }
}

public enum TYPE_OF_RESULT
{
    ALL,
    POSTS,
    USERS,
    HASHTAGS
}

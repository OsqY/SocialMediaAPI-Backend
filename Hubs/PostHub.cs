using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Hubs;

[Authorize]
public class PostHub : Hub
{
    private readonly SocialMediaDbContext _context;

    public PostHub(SocialMediaDbContext context)
    {
        _context = context;
    }

    public async Task AddPost(PostDTO postDTO)
    {
        var username = GetUsername();

        if (string.IsNullOrEmpty(username))
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found.");
            return;
        }

        Post? post = new Post
        {
            Description = postDTO.Description,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
            UserId = user.Id
        };

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("Post added.", post);
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

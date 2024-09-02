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

    public async Task AddPost(string postDescription)
    {
        try
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
                Description = postDescription,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                UserId = user.Id
            };

            PostDTO postData = new PostDTO
            {
                Description = postDescription,
                CreatedDate = post.CreatedDate,
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            await Clients.Others.SendAsync("NewPost", postData);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in AddPost: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while adding your post.");
        }
    }

    public async Task LikeOrUnlikePost(int postId, bool isLikingPost)
    {
        try
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                await Clients.Caller.SendAsync("Error", "That post doesn't exists.");
                return;
            }

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

            if (isLikingPost)
                post.LikedByUsers.Add(user);
            else
                post.LikedByUsers.Remove(user);

            string message = isLikingPost ? $"Post liked by {username}." : "";
            await Clients.Others.SendAsync("PostLiked", message);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in LikePost: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while liking your post.");
        }
    }

    public async Task CommentOnPost(int postId, string commentContent)
    {
        try
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                await Clients.Caller.SendAsync("Error", "That post doesn't exists.");
                return;
            }

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

            Comment comment = new Comment
            {
                UserId = user.Id,
                PostId = post.Id,
                CreatedDate = DateTime.Now,
                Content = commentContent
            };

            post.Comments.Add(comment);

            await Clients.Others.SendAsync("PostCommented", $"{username} commented on your post.");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in LikePost: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while liking your post.");
        }
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

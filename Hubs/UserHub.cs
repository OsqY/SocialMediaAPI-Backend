using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;

namespace SocialMediaAPI.Hubs;

[Authorize]
public class UserHub : Hub
{
    private readonly SocialMediaDbContext _context;

    public UserHub(SocialMediaDbContext context)
    {
        _context = context;
    }

    public async Task FollowUser(string usernameToFollow)
    {
        try
        {
            if (string.IsNullOrEmpty(usernameToFollow))
            {
                await Clients.Caller.SendAsync("Error", "Username to follow is not valid.");
                return;
            }

            var userToFollow = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == usernameToFollow
            );
            if (userToFollow == null)
            {
                await Clients.Caller.SendAsync("Error", "User to follow doesn't exists.");
                return;
            }

            var username = GetUsername();

            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("Error", "User is not authenticated.");
                return;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                await Clients.Caller.SendAsync("Error", "User making the request not found.");
                return;
            }

            userToFollow.Followers.Add(user);

            await Clients.All.SendAsync("NewFollower", $"{user.UserName} is following you.");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in LikePost: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while following an user.");
        }
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

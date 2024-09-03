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
                await Clients.Caller.SendAsync("Error", "User to follow doesn't exist.");
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

            if (!userToFollow.Followers.Any(f => f.Id == user.Id))
            {
                userToFollow.Followers.Add(user);
                await _context.SaveChangesAsync();
                await Clients
                    .User(userToFollow.Id)
                    .SendAsync("NewFollowerNotification", $"{user.UserName} is following you.");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in Following user: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while following a user.");
        }
    }

    public async Task UnfollowUser(string usernameToUnfollow)
    {
        try
        {
            if (string.IsNullOrEmpty(usernameToUnfollow))
            {
                await Clients.Caller.SendAsync("Error", "Username to unfollow is not valid.");
                return;
            }

            var userToUnfollow = await _context
                .Users.Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.UserName == usernameToUnfollow);
            if (userToUnfollow == null)
            {
                await Clients.Caller.SendAsync("Error", "User to unfollow doesn't exist.");
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

            var follower = userToUnfollow.Followers.FirstOrDefault(f => f.Id == user.Id);
            if (follower != null)
            {
                Console.WriteLine("AAa");
                userToUnfollow.Followers.Remove(follower);
                await _context.SaveChangesAsync();
                await Clients
                    .User(userToUnfollow.Id)
                    .SendAsync("NewFollowerNotification", $"{user.UserName} has unfollowed you.");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in Unfollowing user: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while unfollowing a user.");
        }
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

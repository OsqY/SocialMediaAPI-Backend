using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SocialMediaDbContext _context;
    private readonly UserManager<ApiUser> _userManager;

    public ChatHub(SocialMediaDbContext context, UserManager<ApiUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task JoinChatWithUser(string username)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, username);
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _userManager.GetUserAsync(Context.User);

        if (user != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
            await base.OnConnectedAsync();
        }
    }

    public async Task SendMessageToUser(string receiverId, string content)
    {
        try
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return;
            }

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
            {
                await Clients.Caller.SendAsync("Error", "Receiver not found.");
                return;
            }

            var message = new ChatMessage
            {
                Content = content,
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients
                .Group(sender.Id)
                .SendAsync(
                    "NewMessage",
                    new
                    {
                        Id = message.Id,
                        Content = message.Content,
                        SenderId = message.SenderId,
                        ReceiverId = message.ReceiverId,
                        CreatedDate = message.CreatedDate
                    }
                );

            await Clients
                .Group(receiver.Id)
                .SendAsync(
                    "NewMessage",
                    new
                    {
                        Id = message.Id,
                        Content = message.Content,
                        SenderId = message.SenderId,
                        ReceiverId = message.ReceiverId,
                        CreatedDate = message.CreatedDate
                    }
                );
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
        }
    }

    public async Task GetChatMessages(string userId)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(Context.User);

            if (currentUser == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return;
            }

            var messages = _context
                .Messages.Where(m =>
                    (m.SenderId == currentUser.Id && m.ReceiverId == userId)
                    || (m.SenderId == userId && m.ReceiverId == currentUser.Id)
                )
                .OrderBy(m => m.CreatedDate)
                .Select(m => new
                {
                    Content = m.Content,
                    SenderName = m.Sender.UserName,
                    ReceiverName = m.Receiver.UserName,
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("GetChatMessages", messages);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            await Clients.Caller.SendAsync(
                "Error",
                "An error occurred while retrieving your messages."
            );
        }
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

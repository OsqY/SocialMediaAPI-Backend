using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SocialMediaDbContext _context;

    public ChatHub(SocialMediaDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var username = GetUsername();

        if (!string.IsNullOrEmpty(username))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, username);
            await base.OnConnectedAsync();
        }
    }

    public async Task JoinChatWithUser(string username)
    {
        var currentUsername = GetUsername();

        if (!string.IsNullOrEmpty(currentUsername))
        {
            var groupName = GetGroupName(currentUsername, username);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public async Task SendMessageToUser(string receiverUsername, string content)
    {
        try
        {
            var senderUsername = GetUsername();
            if (string.IsNullOrEmpty(senderUsername))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return;
            }

            var sender = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == senderUsername
            );
            var receiver = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == receiverUsername
            );

            if (sender == null || receiver == null)
            {
                await Clients.Caller.SendAsync("Error", "User not found.");
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

            var messageDto = new
            {
                Content = message.Content,
                SenderName = sender.UserName,
                ReceiverName = receiver.UserName,
                CreatedDate = message.CreatedDate
            };

            var groupName = GetGroupName(senderUsername, receiverUsername);
            await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
        }
    }

    public async Task GetChatMessages(string username)
    {
        try
        {
            var currentUsername = GetUsername();

            if (string.IsNullOrEmpty(currentUsername))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return;
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == currentUsername
            );
            var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (currentUser == null || otherUser == null)
            {
                await Clients.Caller.SendAsync("Error", "User not found.");
                return;
            }

            var messages = await _context
                .Messages.Where(m =>
                    (m.SenderId == currentUser.Id && m.ReceiverId == otherUser.Id)
                    || (m.SenderId == otherUser.Id && m.ReceiverId == currentUser.Id)
                )
                .Include(m => m.Sender)
                .AsSplitQuery()
                .Include(m => m.Receiver)
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

    public async Task RetrieveHistory()
    {
        try
        {
            var currentUsername = GetUsername();

            if (string.IsNullOrEmpty(currentUsername))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return;
            }

            var currentUser = await _context
                .Users.Include(u => u.ReceivedMessages)
                .ThenInclude(m => m.Sender)
                .Include(u => u.SentMessages)
                .ThenInclude(m => m.Receiver)
                .FirstOrDefaultAsync(u => u.UserName == currentUsername);

            if (currentUser == null)
            {
                await Clients.Caller.SendAsync("Error", "User not found.");
                return;
            }

            var sentMessages = currentUser.SentMessages.OrderBy(m => m.CreatedDate).ToList();
            var receivedMessages = currentUser
                .ReceivedMessages.OrderBy(m => m.CreatedDate)
                .ToList();

            var lastMessages = new List<LastMessageDTO>();

            foreach (var sentMessage in sentMessages)
            {
                if (!lastMessages.Any(m => m.Username == sentMessage.Receiver.UserName))
                {
                    lastMessages.Add(
                        new LastMessageDTO
                        {
                            Username = sentMessage.Receiver.UserName,
                            LastMessage = sentMessage.Content
                        }
                    );
                }
            }

            foreach (var receivedMessage in receivedMessages)
            {
                if (!lastMessages.Any(m => m.Username == receivedMessage.Sender.UserName))
                {
                    lastMessages.Add(
                        new LastMessageDTO
                        {
                            Username = receivedMessage.Sender.UserName,
                            LastMessage = receivedMessage.Content
                        }
                    );
                }
            }

            await Clients.Caller.SendAsync("RetrieveHistory", lastMessages);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in RetrieveHistory: {e.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while retrieving history.");
        }
    }

    private string GetGroupName(string user1, string user2)
    {
        var stringCompare = string.CompareOrdinal(user1, user2) < 0;
        return stringCompare ? $"{user1}-{user2}" : $"{user2}-{user1}";
    }

    private string? GetUsername()
    {
        return Context
            .User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;
    }
}

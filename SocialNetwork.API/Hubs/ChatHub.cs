using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace SocialNetwork.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SocialDbContext _context;
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(SocialDbContext context, IMessageRepository messageRepository, ILogger<ChatHub> logger)
        {
            _context = context;
            _messageRepository = messageRepository;
            _logger = logger;
        }

        public async Task JoinChat(Guid chatId, Guid userId)
        {
            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == userId);

            if (userChat == null)
                throw new HubException("User is not a member of this chat");

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task SendMessage(Guid chatId, string content)
        {
            var userId = Guid.Parse(Context.User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            var userChat = await _context.UserChats
                .Include(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == userId);

            if (userChat == null)
                throw new HubException("User is not a member of this chat");

            var message = new Message
            {
                SenderId = userId,
                ChatId = chatId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await _messageRepository.CreateAsync(message);

            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", message);

            _logger.LogInformation("User {UserId} sent message to chat {ChatId}", userId, chatId);
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("New connection established: {ConnectionId}", connectionId);

            var userId = Guid.Parse(Context.User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            if(userId != Guid.Empty)
            {
                var userChats = await _context.UserChats
                    .Where(uc => uc.UserId == userId)
                    .ToListAsync();

                foreach (var userChat in userChats)
                {
                    await Groups.AddToGroupAsync(connectionId, userChat.ChatId.ToString());
                }

                _logger.LogInformation("User {UserId} added to {ChatCount} chat groups", userId, userChats.Count);
            }
            else
            {
                _logger.LogWarning("Connection {ConnectionId} has no valid user ID", connectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}

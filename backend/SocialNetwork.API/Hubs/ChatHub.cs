using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace SocialNetwork.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly SocialDbContext _context;
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<ChatHub> _logger;
        private readonly IMapper _mapper;

        public ChatHub(SocialDbContext context, IMessageRepository messageRepository, ILogger<ChatHub> logger, IMapper mapper)
        {
            _context = context;
            _messageRepository = messageRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task JoinChat(Guid chatId, Guid userId)
        {
            try
            {
                var userChat = await _context.UserChats
                    .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == userId);

                if (userChat == null)
                {
                    var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);

                    var userChats = await _context.UserChats
                        .Where(uc => uc.UserId == userId)
                        .Select(uc => uc.ChatId)
                        .ToListAsync();

                    throw new HubException("User is not a member of this chat");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                _logger.LogInformation("JoinChat: User {UserId} successfully added to group {ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JoinChat: Error joining chat {ChatId} for user {UserId}", chatId, userId);
                throw;
            }
        }

        public async Task SendMessage(Guid chatId, string content)
        {
            try
            {
                var userId = Guid.Parse(Context.User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

                var userChat = await _context.UserChats
                    .FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.UserId == userId);

                if (userChat == null)
                {
                    throw new HubException("User not member of chat");
                }

                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    SenderId = userId,
                    ChatId = chatId,
                    Content = content,
                    SentAt = DateTime.UtcNow
                };

                await _messageRepository.CreateAsync(message);

                var messageDto = _mapper.Map<MessageDto>(message);

                await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessage: Error sending message to chat {ChatId}", chatId);
                throw;
            }
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

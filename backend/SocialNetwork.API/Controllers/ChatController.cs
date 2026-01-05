using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Interfaces;
using System.Security.Claims;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;

        public ChatController(ILogger<ChatController> logger, 
            IMessageService messageService, 
            IChatService chatService)
        {
            _logger = logger;
            _messageService = messageService;
            _chatService = chatService;
        }

        [HttpGet("chats")]
        public async Task<IActionResult> MyChats(CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            var chats = await _chatService.GetChatsByUserIdAsync(userId, cancellationToken);
            return Ok(chats);
        }

        [HttpGet("chats/{chatId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId, CancellationToken cancellationToken = default)
        {
            var messages = await _messageService.GetMessageByChatIdAsync(chatId, cancellationToken);
            return Ok(messages);
        }

        [HttpPost("private/{secondUserId:guid}")]
        public async Task<IActionResult> CreatePrivateChat(Guid secondUserId, CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            var chat = await _chatService.CreatePrivateChatAsync(userId, secondUserId, cancellationToken);
            _logger.LogInformation("Private chat created between users {UserId1} and {UserId2} with Chat ID: {ChatId}", userId, secondUserId, chat.Id);
            return Ok(new { chatId = chat.Id });
        }
    }
}

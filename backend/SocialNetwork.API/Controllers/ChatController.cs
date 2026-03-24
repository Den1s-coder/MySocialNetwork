using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.DTO.Chats;
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
        private readonly IMapper _mapper;

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
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
            var messages = await _messageService.GetMessageByChatIdAsync(chatId, userId, cancellationToken);
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

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest request, CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Chat title is required");

            var chat = await _chatService.CreateGroupChatAsync(request.Title, userId, cancellationToken);
            _logger.LogInformation("Group chat created: {ChatId} by user {UserId}", chat.Id, userId);
            return Ok(new { chatId = chat.Id });
        }

        [HttpPost("group/{chatId:guid}/members/{userId:guid}")]
        public async Task<IActionResult> AddUserToGroup(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var requesterId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

                await _chatService.AddUserToChatAsync(chatId, userId, requesterId, cancellationToken);
                _logger.LogInformation("User {UserId} added to group chat {ChatId} by {RequesterId}", userId, chatId, requesterId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add user {UserId} to chat {ChatId}", userId, chatId);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("group/{chatId:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveUserFromGroup(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var requesterId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

                await _chatService.RemoveUserFromChatAsync(chatId, userId, cancellationToken);
                _logger.LogInformation("User {UserId} removed from group chat {ChatId} by {RequesterId}", userId, chatId, requesterId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove user {UserId} from chat {ChatId}", userId, chatId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("group/{chatId:guid}/members/{userId:guid}/role")]
        public async Task<IActionResult> ChangeUserRole(Guid chatId, Guid userId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken = default)
        {
            var requesterId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            await _chatService.ChangeUserRoleInChatAsync(chatId, userId, request.NewRole, cancellationToken);
            _logger.LogInformation("User {UserId} role changed in chat {ChatId} by {RequesterId}", userId, chatId, requesterId);
            return NoContent();
        }

        [Authorize]
        [HttpPost("{messageId:guid}/react")]
        public async Task<IActionResult> ToggleReaction(Guid messageId, [FromQuery] Guid ReactionTypeId, CancellationToken cancellationToken = default)
        {
            var sid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (!Guid.TryParse(sid, out var userId))
                return Unauthorized();

            await _messageService.ToogleReactionAsync(messageId, userId, ReactionTypeId, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{chatId:guid}")]
        public async Task<IActionResult> DeleteChat(Guid chatId, CancellationToken cancellationToken = default)
        {
            var requesterId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.Sid).Value);

            await _chatService.DeleteChatAsync(chatId, requesterId, cancellationToken);
            _logger.LogInformation("Chat {ChatId} deleted by user {RequesterId}", chatId, requesterId);
            return NoContent();
        }
    }
}

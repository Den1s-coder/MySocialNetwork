using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMessageRepository messageRepository, IMapper mapper, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetMessagesByChatIdAsync(chatId, cancellationToken);
            var messageDtos = _mapper.Map<IEnumerable<MessageDto>>(messages);

            foreach (var dto in messageDtos)
            {
                var message = messages.FirstOrDefault(m => m.Id == dto.Id);
                if (message != null)
                {
                    var userReaction = message.Reactions.FirstOrDefault(r => r.UserId == userId);
                    if (userReaction != null)
                    {
                        dto.CurrentUserReactionCode = userReaction.ReactionType.Code;
                    }
                }
            }

            return messageDtos;
        }

        public async Task ToogleReactionAsync(Guid messageId, Guid userId, Guid reactionType, CancellationToken cancellationToken = default)
        {
            var existingMessage = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
            if (existingMessage == null)
            {
                _logger.LogWarning("Attempted to toggle reaction for non-existent message with ID: " + messageId);
                return;
            }

            await _messageRepository.ToggleReactionAsync(messageId, userId, reactionType, cancellationToken);
        }
    }
}

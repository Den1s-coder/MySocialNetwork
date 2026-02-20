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

        public async Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatid, CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetMessagesByChatIdAsync(chatid);

            return _mapper.Map<IEnumerable<Message>, IEnumerable<MessageDto>>(messages);
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

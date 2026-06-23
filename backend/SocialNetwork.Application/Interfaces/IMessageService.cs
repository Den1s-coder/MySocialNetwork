using SocialNetwork.Application.DTO.Chats;

namespace SocialNetwork.Application.Interfaces
{
    public interface IMessageService
    {
        public Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
        public Task ToogleReactionAsync(Guid messageId, Guid userId, Guid reactionType, CancellationToken cancellationToken = default);
    }
}

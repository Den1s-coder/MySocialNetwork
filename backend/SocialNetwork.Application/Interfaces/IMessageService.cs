using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface IMessageService
    {
        public Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatid, CancellationToken cancellationToken = default);
        public Task ToogleReactionAsync(Guid messageId, Guid userId, Guid reactionType, CancellationToken cancellationToken = default);
    }
}

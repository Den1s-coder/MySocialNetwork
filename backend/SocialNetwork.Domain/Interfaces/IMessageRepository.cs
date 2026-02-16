using SocialNetwork.Domain.Entities.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IMessageRepository: IGenerycRepository<Message>
    {
        public Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Message>> GetMessagesBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Message>> SearchMessagesInChatAsync(Guid chatId, string searchTerm, CancellationToken cancellationToken = default);
        public Task ToggleReactionAsync(Guid messageId, Guid userId, Guid reactionType, CancellationToken cancellationToken = default);
    }
}

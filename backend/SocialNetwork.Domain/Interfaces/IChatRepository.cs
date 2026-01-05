using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IChatRepository: IGenerycRepository<Chat>
    {
        public Task<IEnumerable<Chat>> GetChatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task<Chat?> GetChatWithMessagesAsync(Guid chatId, CancellationToken cancellationToken = default);
        public Task<Chat?> GetChatBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    }
}

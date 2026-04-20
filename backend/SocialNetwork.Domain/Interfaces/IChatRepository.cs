using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IChatRepository : IGenerycRepository<Chat>
    {
        public Task<IEnumerable<Chat>> GetChatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task<Chat?> GetChatWithMessagesAsync(Guid chatId, CancellationToken cancellationToken = default);
        public Task<Chat?> GetChatBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
        public Task AddUserToChatAsync(Guid chatId, Guid userId, ChatRole role, CancellationToken cancellationToken = default);
        public Task RemoveUserFromChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    }
}

using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IChatService
    {
        public Task<Chat> CreatePrivateChatAsync(Guid userId1, Guid userId2);
        public Task<Chat> CreateGroupChatAsync(string title, Guid ownerId);
        public Task<Chat> CreateChannelChatAsync(string title, Guid ownerId);
        public Task AddUserToChatAsync(Guid chatId, Guid userId);
        public Task RemoveUserFromChatAsync(Guid chatId, Guid userId);
        public Task ChangeUserRoleInChatAsync(Guid chatId, Guid userId, int newRole);
    }
}

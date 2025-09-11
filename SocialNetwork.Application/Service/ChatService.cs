using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task AddUserToChatAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat == null)
                throw new Exception("Chat not found");

            if(chat.Type == ChatType.Private)
                throw new Exception("Cannot add users to a private chat");

            if (chat.UserChats.Any(uc => uc.UserId == userId))
                throw new Exception("User already in chat");

            var userChat = new UserChat
            {
                ChatId = chat.Id,
                UserId = userId,
                Role = ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            chat.UserChats.Add(userChat);

            await _chatRepository.UpdateAsync(chat);
        }

        public async Task ChangeUserRoleInChatAsync(Guid chatId, Guid userId, int newRole)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);

            if (chat == null)
                throw new Exception("Chat not found");

            var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId == userId);

            if (userChat == null)
                throw new Exception("User not found in chat");

            userChat.Role = (ChatRole)newRole;
        }

        public async Task<Chat> CreateChannelChatAsync(string title, Guid ownerId)
        {
            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                Title = title,
                Type = ChatType.Channel,
                CreatedAt = DateTime.UtcNow,
            };

            var ownerUserChat = new UserChat
            {
                ChatId = chat.Id,
                UserId = ownerId,
                Role = ChatRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            chat.UserChats.Add(ownerUserChat);

            await _chatRepository.CreateAsync(chat);

            return chat;
        }

        public async Task<Chat> CreateGroupChatAsync(string title, Guid ownerId)
        {
            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                Title = title,
                Type = ChatType.Group,
                CreatedAt = DateTime.UtcNow,
            };

            var ownerUserChat = new UserChat
            {
                ChatId = chat.Id,
                UserId = ownerId,
                Role = ChatRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            chat.UserChats.Add(ownerUserChat);

            await _chatRepository.CreateAsync(chat);

            return chat;
        }

        public async Task<Chat> CreatePrivateChatAsync(Guid userId1, Guid userId2)
        {
            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                Title = "Private Chat",
                Type = ChatType.Private,
                CreatedAt = DateTime.UtcNow,
            };

            var ownerUserChat = new UserChat
            {
                ChatId = chat.Id,
                UserId = userId1,
                Role = ChatRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            var memberUserChat = new UserChat
            {
                ChatId = chat.Id,
                UserId = userId2,
                Role = ChatRole.Owner, 
                JoinedAt = DateTime.UtcNow
            };

            chat.UserChats.Add(ownerUserChat);
            chat.UserChats.Add(memberUserChat);

            await _chatRepository.CreateAsync(chat);

            return chat;
        }

        public async Task RemoveUserFromChatAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);

            if (chat == null)
                throw new Exception("Chat not found");

            var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId == userId);

            if (userChat == null)
                throw new Exception("User not found in chat");

            if (userChat.Role == ChatRole.Owner)
                throw new Exception("Cannot remove the owner from the chat");

            chat.UserChats.Remove(userChat);

            await _chatRepository.UpdateAsync(chat);
        }
    }
}

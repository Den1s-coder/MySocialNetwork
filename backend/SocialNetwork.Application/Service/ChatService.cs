using AutoMapper;
using SocialNetwork.Application.DTO;
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
        private readonly IMapper _mapper;

        public ChatService(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
        }

        public async Task AddUserToChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
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

        public async Task ChangeUserRoleInChatAsync(Guid chatId, Guid userId, int newRole, CancellationToken cancellationToken = default)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);

            if (chat == null)
                throw new Exception("Chat not found");

            var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId == userId);

            if (userChat == null)
                throw new Exception("User not found in chat");

            userChat.Role = (ChatRole)newRole;
        }

        public async Task<ChatDto> CreateChannelChatAsync(string title, Guid ownerId, CancellationToken cancellationToken = default)
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

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto> CreateGroupChatAsync(string title, Guid ownerId, CancellationToken cancellationToken = default)
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

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto> CreatePrivateChatAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
        {
            var existing = await _chatRepository.GetChatBetweenUsersAsync(userId1, userId2);
            if (existing != null)
                return _mapper.Map<ChatDto>(existing);

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

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task RemoveUserFromChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
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

        public async Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var chats = await _chatRepository.GetChatsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }
    }
}

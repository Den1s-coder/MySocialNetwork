using AutoMapper;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;


namespace SocialNetwork.Application.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;

        public ChatService(IChatRepository chatRepository, IFriendshipRepository friendshipRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task AddUserToChatAsync(Guid chatId, Guid userId, Guid requesterId, CancellationToken cancellationToken = default)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
            if (chat == null)
                throw new Exception("Chat not found");

            if (chat.Type == ChatType.Private)
                throw new Exception("Cannot add users to a private chat");

            var areFriends = await _friendshipRepository.AreFriendsAsync(requesterId, userId, cancellationToken);
            if (!areFriends)
                throw new Exception("User must be a friend to be added to this chat");

            await _chatRepository.AddUserToChatAsync(chatId, userId, ChatRole.Member, cancellationToken);
        }

        public async Task ChangeUserRoleInChatAsync(Guid chatId, Guid userId, int newRole, CancellationToken cancellationToken = default)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);

            if (chat == null)
                throw new Exception("Chat not found");

            var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId == userId);

            if (userChat == null)
                throw new Exception("User not found in chat");

            userChat.Role = (ChatRole)newRole;
            
            await _chatRepository.UpdateAsync(chat, cancellationToken);
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

            await _chatRepository.CreateAsync(chat, cancellationToken);

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

            await _chatRepository.CreateAsync(chat, cancellationToken);

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto> CreatePrivateChatAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
        {
            var existing = await _chatRepository.GetChatBetweenUsersAsync(userId1, userId2, cancellationToken);
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

            await _chatRepository.CreateAsync(chat, cancellationToken);

            return _mapper.Map<ChatDto>(chat);
        }

        public async Task RemoveUserFromChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);

            if (chat == null)
                throw new Exception("Chat not found");

            await _chatRepository.RemoveUserFromChatAsync(chatId, userId, cancellationToken);
        }

        public async Task<IEnumerable<ChatDto>> GetChatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var chats = await _chatRepository.GetChatsByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Repos
{
    public class ChatRepository : IChatRepository
    {
        private readonly SocialDbContext _context;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(SocialDbContext context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Chat created with ID: " + chat.Id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null)
            {
                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Chat deleted with ID: " + id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent chat with ID: " + id);
            }
        }

        public async Task<IEnumerable<Chat>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Chats
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Chats
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Chat?> GetChatBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
        {
            return await _context.Chats
                .Include(c => c.UserChats)
                    .ThenInclude(uc => uc.User)
                .Where(c => c.Type == ChatType.Private)
                .FirstOrDefaultAsync(c =>
                    c.UserChats.Any(uc => uc.UserId == userId1) &&
                    c.UserChats.Any(uc => uc.UserId == userId2));
        }

        public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Chats
                .Include(c => c.UserChats)
                    .ThenInclude(uc => uc.User)
                .Where(c => c.UserChats.Any(p => p.UserId == userId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Chat?> GetChatWithMessagesAsync(Guid chatId, CancellationToken cancellationToken = default)
        {
            return await _context.Chats
                .Include(c => c.Messages)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
        }
    }
}

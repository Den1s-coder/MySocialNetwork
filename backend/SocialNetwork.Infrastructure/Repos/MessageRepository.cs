using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Entities.Posts;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Repos
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SocialDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(SocialDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(Message message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("MessageRepository.CreateAsync: Creating message {MessageId}", message.Id);
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                _logger.LogInformation("MessageRepository.CreateAsync: Message {MessageId} created successfully", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MessageRepository.CreateAsync: Error creating message {MessageId}", message.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Message deleted with ID: " + id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent message with ID: " + id);
            }
        }

        public async Task<IEnumerable<Message>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Sender)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Where(m => m.SenderId == senderId)
                .Include(m => m.Sender)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> SearchMessagesInChatAsync(Guid chatId, string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId && m.Content.Contains(searchTerm))
                .Include(m => m.Sender)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleReactionAsync(Guid messageId, Guid UserId, Guid ReactionTypeId, CancellationToken cancellationToken = default)
        {
            var existingReaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == UserId, cancellationToken);

            if (existingReaction == null)
            {
                var newReaction = new MessageReaction(UserId, messageId, ReactionTypeId);
                _context.MessageReactions.Add(newReaction);
            }
            else if (existingReaction.ReactionTypeId == ReactionTypeId)
            {
                _context.MessageReactions.Remove(existingReaction);
            }
            else
            {
                existingReaction.ReactionTypeId = ReactionTypeId;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

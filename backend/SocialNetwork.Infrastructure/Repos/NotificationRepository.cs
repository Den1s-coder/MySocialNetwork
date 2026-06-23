using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Infrastructure.Repos
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SocialDbContext _context;

        public NotificationRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Notification T, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Add(T);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
            if (entity == null) return;
            _context.Notifications.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Notification T, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Update(T);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

using Microsoft.Extensions.Logging;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            if (notification == null) return Task.CompletedTask;
            if (notification.Id == Guid.Empty) notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            _logger.LogDebug("CreateAsync: notification {NotificationId} for user {UserId}", notification.Id, notification.UserId);
            return _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        public Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);
        }

        public Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            if (notification == null)
            {
                _logger.LogWarning("MarkAsReadAsync: notification not found {NotificationId} (requested by user {UserId})", notificationId, userId);
                throw new ArgumentException("Notification not found");
            }

            // логируем реальные значения для диагностики
            _logger.LogDebug("MarkAsReadAsync: notification.UserId={NotificationUserId}, callerUserId={CallerUserId}, notificationId={NotificationId}",
                notification.UserId, userId, notificationId);

            if (notification.UserId != userId)
            {
                _logger.LogWarning("MarkAsReadAsync: user {CallerUserId} tried to mark notification {NotificationId} owned by {NotificationUserId}", userId, notificationId, notification.UserId);
                throw new UnauthorizedAccessException("Notification does not belong to user");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification, cancellationToken);
                _logger.LogInformation("MarkAsReadAsync: notification {NotificationId} marked read by {UserId}", notificationId, userId);
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var unread = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
            var tasks = new List<Task>();
            foreach (var n in unread)
            {
                n.IsRead = true;
                tasks.Add(_notificationRepository.UpdateAsync(n, cancellationToken));
            }
            _logger.LogInformation("MarkAllAsReadAsync: user {UserId} marked {Count} notifications as read", userId, unread?.Count() ?? 0);
            await Task.WhenAll(tasks);
        }
    }
}

using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }   

        public Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
        }

        public Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            if(notification == null) return Task.CompletedTask;
            if(notification.Id == Guid.Empty) notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            return _notificationRepository.CreateAsync(notification, cancellationToken);
        }
    }
}

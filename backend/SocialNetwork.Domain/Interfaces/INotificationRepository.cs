using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Domain.Interfaces
{
    public interface INotificationRepository : IGenerycRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

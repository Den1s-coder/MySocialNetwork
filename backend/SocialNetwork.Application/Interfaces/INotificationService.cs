using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

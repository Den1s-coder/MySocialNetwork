using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.API.Hubs;
using Microsoft.Extensions.Logging;
using SocialNetwork.Domain.Entities;
using System.Text.Json;

namespace SocialNetwork.API.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationPublisher> _logger;

        public NotificationPublisher(IHubContext<NotificationHub> hubContext, INotificationService notificationService, ILogger<NotificationPublisher> logger)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task PublishToUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", payload, cancellationToken);

                var msg = JsonSerializer.Serialize(payload);
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Message = msg,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.SendNotificationAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishToUserAsync failed for user {UserId}", userId);
            }
        }

        public async Task PublishToUsersAsync(IEnumerable<Guid> userIds, object payload, CancellationToken cancellationToken = default)
        {
            var tasks = userIds.Select(id => PublishToUserAsync(id, payload, cancellationToken));
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishToUsersAsync failed");
            }
        }
    }
}
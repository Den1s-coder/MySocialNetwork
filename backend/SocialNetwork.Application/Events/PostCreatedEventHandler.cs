using MediatR;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Events
{
    public class PostCreatedEventHandler : INotificationHandler<PostCreatedEvent>
    {
        private readonly ILogger<PostCreatedEventHandler> _logger;
        private readonly IFriendService _friendService;
        private readonly IUserService _userService;
        private readonly Interfaces.INotificationPublisher _notificationPublisher;

        public PostCreatedEventHandler(
            ILogger<PostCreatedEventHandler> logger,
            IFriendService friendService,
            IUserService userService,
            Interfaces.INotificationPublisher notificationPublisher)
        {
            _logger = logger;
            _friendService = friendService;
            _userService = userService;
            _notificationPublisher = notificationPublisher;
        }

        public async Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Post created with ID: {notification.PostId} at {notification.CreatedAt} \n Send message to friends");

                var author = await _userService.GetByIdAsync(notification.AuthorId, cancellationToken);
                if (author == null)
                {
                    _logger.LogWarning("Author not found for PostId {PostId}", notification.PostId);
                    return;
                }

                var friends = await _friendService.GetFriendsOfUser(notification.AuthorId, cancellationToken);
                
                if (friends == null || !friends.Any())
                {
                    _logger.LogInformation("No friends to notify for user {UserId}", notification.AuthorId);
                    return;
                }

                var recipientIds = friends
                    .Select(f => f.Id)
                    .Where(id => id != Guid.Empty && id != notification.AuthorId)
                    .Distinct()
                    .ToList();

                if (!recipientIds.Any())
                {
                    _logger.LogInformation("No recipients to notify for PostId {PostId}", notification.PostId);
                    return;
                }

                var payload = new
                {
                    type = "post_created",
                    postId = notification.PostId,
                    authorId = notification.AuthorId,
                    authorName = author.Name,
                    createdAt = notification.CreatedAt
                };

                await _notificationPublisher.PublishToUsersAsync(recipientIds, payload, cancellationToken);

                _logger.LogInformation("Sent post created notification to {RecipientCount} users for PostId {PostId}", 
                    recipientIds.Count, notification.PostId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling PostCreatedEvent for PostId {PostId}", notification.PostId);
            }
        }
    }
}

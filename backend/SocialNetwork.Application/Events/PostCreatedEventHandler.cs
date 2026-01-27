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
        public PostCreatedEventHandler(ILogger<PostCreatedEventHandler> logger,IFriendService friendService, IUserService userService, Interfaces.INotificationPublisher notificationPublisher)
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
                _logger.LogInformation($"Post created with ID: {notification.PostId} at {notification.CreatedAt} \n Send message to follows");

                var author = await _userService.GetByIdAsync(notification.AuthorId, cancellationToken);
                if (author == null) return;

                var friendships = await _friendService.GetFriendsOfUser(author.Id);

                var recipientIds = friendships
                    .Where(f => f.Status == Domain.Enums.FriendshipStatus.Accepted)
                    .Select(f => f.RequesterId == notification.AuthorId ? f.AddresseeId : f.RequesterId)
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (!recipientIds.Any())
                {
                    _logger.LogInformation("No friends to notify");
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

                _logger.LogInformation("Sent Post-created notify");
            }
            catch (Exception ex) 
            {
                _logger.LogInformation(ex, "Error while handling PostCreatedEvent for PostId {PostId}", notification.PostId);
            }
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Events
{
    public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
    {
        private readonly ILogger<CommentCreatedEventHandler> _logger;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly Interfaces.INotificationPublisher _notificationPublisher;
        public CommentCreatedEventHandler(
            ILogger<CommentCreatedEventHandler> logger,
            IPostService postService,
            IUserService userService,
            Interfaces.INotificationPublisher notificationPublisher
            )
        {
            _logger = logger;
            _postService = postService;
            _userService = userService;
            _notificationPublisher = notificationPublisher;
        }
        public async Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Comment created: {CommentId} on Post {PostId} by {AuthorId} at {CreatedAt}",
                            notification.CommentId, notification.PostId, notification.AuthorId, notification.CreatedAt);

            try
            {
                var post = await _postService.GetByIdAsync(notification.PostId, cancellationToken);
                if (post == null)
                {
                    _logger.LogWarning("Post not found: {PostId}", notification.PostId);
                    return;
                }

                if(post.AuthorId == notification.AuthorId)
                {
                    _logger.LogInformation("Author commented on their own post. No notification sent.");
                    return;
                }

                var author = await _userService.GetByIdAsync(notification.AuthorId, cancellationToken);
                
                var payload = new
                {
                    type = "comment_created",
                    commentId = notification.CommentId,
                    postId = notification.PostId,
                    authorId = notification.AuthorId,
                    authorName = author?.Name,
                    createdAt = notification.CreatedAt
                };

                await _notificationPublisher.PublishToUserAsync(post.AuthorId, payload, cancellationToken);

                _logger.LogInformation("Sent Comment-created notification to Post Author: {AuthorId}", post.AuthorId);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error handling CommentCreatedEvent for CommentId: {CommentId}", notification.CommentId);
            }
        }
    }
}

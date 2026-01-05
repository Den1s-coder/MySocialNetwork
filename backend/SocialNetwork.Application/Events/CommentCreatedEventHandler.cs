using MediatR;
using Microsoft.Extensions.Logging;

namespace SocialNetwork.Application.Events
{
    public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
    {
        private readonly ILogger<CommentCreatedEventHandler> _logger;
        public CommentCreatedEventHandler(ILogger<CommentCreatedEventHandler> logger)
        {
            _logger = logger;
        }
        public Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Comment created with ID: {notification.CommentId} on Post ID: {notification.PostId} at {notification.CreatedAt} \n Notify post author");
            return Task.CompletedTask;
        }
    }
}

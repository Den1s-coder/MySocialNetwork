using MediatR;
using Microsoft.Extensions.Logging;
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
        public PostCreatedEventHandler(ILogger<PostCreatedEventHandler> logger)
        {
            _logger = logger;
        }
        public Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Post created with ID: {notification.PostId} at {notification.CreatedAt} \n Send message to follows");

            return Task.CompletedTask;
        }
    }
}

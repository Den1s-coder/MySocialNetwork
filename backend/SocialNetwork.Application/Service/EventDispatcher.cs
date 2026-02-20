using MediatR;
using SocialNetwork.Application.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IMediator _mediator;
        public EventDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            if (@event == null)
                return Task.CompletedTask;

            return _mediator.Publish(@event, cancellationToken);
        }
    }
}

namespace SocialNetwork.Application.Interfaces
{
    public interface IEventDispatcher
    {
        Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}
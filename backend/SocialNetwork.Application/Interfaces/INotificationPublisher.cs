namespace SocialNetwork.Application.Interfaces
{
    public interface INotificationPublisher
    {
        Task PublishToUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default);
        Task PublishToUsersAsync(IEnumerable<Guid> userIds, object payload, CancellationToken cancellationToken = default);
    }
}

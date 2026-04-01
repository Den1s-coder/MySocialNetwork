using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Domain.Entities.Users;

namespace SocialNetwork.Application.Interfaces
{
    public interface IFriendService
    {
        Task<IEnumerable<Friendship>> GetAllFriends(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetFriendsOfUser(Guid userId, CancellationToken cancellationToken = default);
        Task SendFriendRequest(FriendRequestDto request, CancellationToken cancellationToken = default);
        Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests(Guid userId, CancellationToken cancellationToken = default);
        Task AcceptFriendRequest(Guid requestId, CancellationToken cancellationToken = default);
        Task DeclineFriendRequest(Guid requestId, CancellationToken cancellationToken = default);
        Task RemoveFriend(Guid userId, Guid friendId, CancellationToken cancellationToken = default);
    }
}

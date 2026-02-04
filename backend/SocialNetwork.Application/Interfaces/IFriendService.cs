using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IFriendService
    {
        public Task<IEnumerable<Friendship>> GetAllFriends(CancellationToken cancellationToken = default);
        public Task<IEnumerable<Friendship>> GetFriendsOfUser(Guid userId, CancellationToken cancellationToken = default);
        public Task SendFriendRequest(FriendRequestDto request, CancellationToken cancellationToken = default);
        public Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests(Guid userId, CancellationToken cancellationToken = default);
        public Task AcceptFriendRequest(Guid requestId, CancellationToken cancellationToken = default);
        public Task DeclineFriendRequest(Guid requestId, CancellationToken cancellationToken = default);
        public Task RemoveFriend(Guid userId, Guid friendId);

    }
}

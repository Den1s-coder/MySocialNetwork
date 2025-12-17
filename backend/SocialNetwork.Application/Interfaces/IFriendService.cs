using SocialNetwork.Application.DTO;
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
        public Task<IEnumerable<Friendship>> GetAllFriends();
        public Task<IEnumerable<Friendship>> GetFriendsOfUser(Guid userId);
        public Task SendFriendRequest(FriendRequestDto request);
        public Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests(Guid userId);
        public Task AcceptFriendRequest(Guid requestId);
        public Task DeclineFriendRequest(Guid requestId);
        public Task RemoveFriend(Guid userId, Guid friendId);

    }
}

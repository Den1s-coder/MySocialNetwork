using SocialNetwork.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IFriendService
    {
        public Task<IEnumerable<UserDto>> GetAllFriends();
        public Task<IEnumerable<UserDto>> GetFriendsOfUser(Guid userId);
        public Task SendFriendRequest(Guid addresseeId);
        public Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests();
        public Task AcceptFriendRequest(Guid requestId);
        public Task DeclineFriendRequest(Guid requestId);
        public Task RemoveFriend(Guid friendId);

    }
}

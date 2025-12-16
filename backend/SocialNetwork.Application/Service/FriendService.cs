using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class FriendService : IFriendService
    {
        public Task AcceptFriendRequest(Guid requestId)
        {
            throw new NotImplementedException();
        }

        public Task DeclineFriendRequest(Guid requestId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserDto>> GetAllFriends()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserDto>> GetFriendsOfUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests()
        {
            throw new NotImplementedException();
        }

        public Task RemoveFriend(Guid friendId)
        {
            throw new NotImplementedException();
        }

        public Task SendFriendRequest(Guid addresseeId)
        {
            throw new NotImplementedException();
        }
    }
}

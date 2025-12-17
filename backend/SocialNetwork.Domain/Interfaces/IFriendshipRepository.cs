using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IFriendshipRepository: IGenerycRepository<Friendship>
    {
        Task<bool> AreFriendsAsync(Guid userId1, Guid userId2);
        Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(Guid userId);
    }
}

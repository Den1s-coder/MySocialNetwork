using SocialNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class Friendship: BaseEntity
    {
        public Guid RequesterId { get; set; }
        public User Requester { get; set; }
        public Guid AddresseeId { get; set; }
        public User Addressee { get; set; }
        public DateTime CreatedAt { get; set; }
        public FriendshipStatus Status { get; set; }

    }
}

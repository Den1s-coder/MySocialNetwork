using SocialNetwork.Domain.Entities.Chats;
using SocialNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities.Users
{
    public class UserChat
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public DateTime JoinedAt { get; set; }
        public ChatRole Role { get; set; }
        public ChatRights Rights { get; set; } = ChatRights.None;
    }
}

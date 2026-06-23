using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities.Chats
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public ChatType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}

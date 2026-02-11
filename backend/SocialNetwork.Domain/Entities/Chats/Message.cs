using SocialNetwork.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities.Chats
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public Guid SenderId { get; set; }
        public User Sender { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; } 
        public ICollection<MessageReaction> Reactions { get; set; }

        public Message() { } //for EF migrations

        public Message(Guid chatId, Guid senderId, string content)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            SenderId = senderId;
            Content = content;
            SentAt = DateTime.UtcNow;
            Reactions = new List<MessageReaction>();
        }
    }
}

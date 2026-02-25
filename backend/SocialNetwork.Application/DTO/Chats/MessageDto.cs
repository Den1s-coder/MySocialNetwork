using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Chats
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}

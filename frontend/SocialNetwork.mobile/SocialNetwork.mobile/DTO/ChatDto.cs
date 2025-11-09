using System;
using System.Collections.Generic;

namespace SocialNetwork.mobile.DTO
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserChatDto> UserChats { get; set; }

        // UI helpers
        public List<string> ParticipantNames { get; set; } = new List<string>();
        public string DisplayTitle { get; set; }
    }
}

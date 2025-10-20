﻿using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Application.DTO
{
    public class UserChatDto
    {
        public Guid UserId { get; set; }
        public Guid ChatId { get; set; }
        public DateTime JoinedAt { get; set; }
        public ChatRole Role { get; set; }
        public ChatRights Rights { get; set; }
    }
}

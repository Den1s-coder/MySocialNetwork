using System;

namespace SocialNetwork.mobile.DTO
{
    public class UserChatDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
    }
}

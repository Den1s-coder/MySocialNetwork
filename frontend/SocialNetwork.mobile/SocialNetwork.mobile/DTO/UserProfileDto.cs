using System;
using System.Collections.Generic;
using System.Text;

namespace SocialNetwork.mobile.DTO
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
    }
}

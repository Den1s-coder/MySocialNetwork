using System;

namespace SocialNetwork.mobile.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsBanned { get; set; }
    }
}

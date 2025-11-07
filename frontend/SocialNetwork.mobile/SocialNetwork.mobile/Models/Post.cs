using System;
using System.Collections.Generic;

namespace SocialNetwork.mobile.Models
{
    public class Post
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string UserName { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsBanned { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class Post
    {
        public string Text {  get; set; }
        public string UserId {  get; set; }
        public User User { get; set; }
        public List<Comment> Comments { get; set; }

        public Post(string text, string userId, User user) 
        { 
            Text = text;
            UserId = userId;
            User = user;
            Comments = new List<Comment>();
        }



    }
}

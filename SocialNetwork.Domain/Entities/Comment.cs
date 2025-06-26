using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public User Author { get; set; }

        public Comment(int id, string text, User user)
        {
            Id = id;
            Text = text;
            Author = user;
        }
    }
}

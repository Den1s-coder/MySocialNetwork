using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Text { get; set; }
        public User Author { get; set; }

        public Comment(int id, string text, User user)
        {
            Text = text;
            Author = user;
        }
    }
}

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
        public Guid AuthorId { get; set; }
        public User Author { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        public bool IsBanned { get; set; } = false;

        public Comment() { } // for EF migrations

        public Comment( string text, Guid user)
        {
            Id = Guid.NewGuid();
            Text = text;
            AuthorId = user;
        }
    }
}

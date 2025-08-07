using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class Post : BaseEntity
    {
        public string Text {  get; set; }
        public Guid UserId {  get; set; }
        public User User { get; set; }
        public List<Comment> Comments { get; set; }

        public Post() { } //for EF migrations

        public Post(string text, Guid userId, User user) 
        { 
            Id = Guid.NewGuid();
            Text = text;
            UserId = userId;
            User = user;
            Comments = new List<Comment>();
        }



    }
}

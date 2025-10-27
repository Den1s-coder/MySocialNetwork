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
        public string? ImageUrl {  get; set; }
        public Guid UserId {  get; set; }
        public User User { get; set; }
        public List<Comment> Comments { get; set; }
        public bool IsBanned { get; set; } = false;

        public Post() { } //for EF migrations

        public Post(string text, Guid userId) 
        { 
            Id = Guid.NewGuid();
            Text = text;
            UserId = userId;
            Comments = new List<Comment>();
        }
    }
}

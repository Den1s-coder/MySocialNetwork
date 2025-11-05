using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public List<Post> UserPosts { get; set; }
        public List<Comment> UserComments { get; set; }
        public bool IsBanned { get; set; } = false;

        public List<UserChat>? UserChats { get; set; } = new List<UserChat>();
        public List<Message>? Messages { get; set; } = new List<Message>();

        public List<Friendship>? RequestedFriendship { get; set; } = new List<Friendship>();
        public List<Friendship>? ReceivedFriendship { get; set; } = new List<Friendship>();

        public User() { } // for EF migrations

        public User( string name, string email, string passwordHash) 
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            UserPosts = new List<Post>();
        }
    }
}

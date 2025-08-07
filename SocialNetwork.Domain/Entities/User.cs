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
        public List<Post> UserPosts { get; set; }

        private User( string name, string email, string passwordHash) 
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            UserPosts = new List<Post>();
        }

        public User CreateUser(string Name, string Email, string PasswordHash) 
        {
            return new User( Name, Email, PasswordHash);
        }
    }
}

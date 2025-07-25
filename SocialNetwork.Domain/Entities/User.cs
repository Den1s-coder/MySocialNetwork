using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public List<Post> UserPosts { get; set; }

        private User( string name, string email, string password) 
        {
            Name = name;
            Email = email;
            PasswordHash = password;
            UserPosts = new List<Post>();
        }

        public User CreateUser(string Name, string Email, string Password) 
        {
            return new User( Name, Email, Password);
        }
    }
}

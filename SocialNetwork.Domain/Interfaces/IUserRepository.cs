using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task Add(User user);
        public Task Update(User user);
        public Task Delete(int id);
        public Task<User> GetById(int id);
    }
}

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
        public Task CreateAsync(User user);
        public Task UpdateAsync(User user);
        public Task DeleteAsync(Guid id);
        public Task<User?> GetById(Guid id);
        public Task<IEnumerable<User>> GetAllAsync();
    }
}

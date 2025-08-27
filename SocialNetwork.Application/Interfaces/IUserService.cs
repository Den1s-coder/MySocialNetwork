using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User?> GetByIdAsync(Guid id);
        public Task BanUser(Guid id);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task<User?> GetUserByNameAsync(string name);
    }
}

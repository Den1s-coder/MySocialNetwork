using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class UserService : IUserService
    {
        public Task BanUser(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(CreateUserDto userDto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllUserIdsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}

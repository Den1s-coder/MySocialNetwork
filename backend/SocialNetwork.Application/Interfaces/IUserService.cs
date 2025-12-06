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
        public Task<IEnumerable<UserDto>> GetAllUsersAsync();
        public Task<UserDto> GetByIdAsync(Guid id);
        public Task BanUser(Guid id);
        public Task<UserDto> GetUserByEmailAsync(string email);
        public Task<UserDto> GetUserByNameAsync(string name);
        public Task UpdateProfileAsync(UserDto updatedUserDto);
    }
}

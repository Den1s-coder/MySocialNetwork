using SocialNetwork.Application.DTO.Users;
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
        public Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        public Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task BanUser(Guid id, CancellationToken cancellationToken = default);
        public Task<UserDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        public Task<UserDto> GetUserByNameAsync(string name, CancellationToken cancellationToken = default);
        public Task UpdateProfileAsync(UserDto updatedUserDto, CancellationToken cancellationToken = default);
    }
}

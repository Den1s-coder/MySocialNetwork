using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaginetedResult<UserDto>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
        Task UpdateAsync(Guid id, UserDto userDto, CancellationToken cancellationToken = default);
        Task BanUserAsync(Guid id, CancellationToken cancellationToken = default);
        Task ChangeRoleAsync(Guid id, string newRole, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);
        Task ChangeEmailAsync(Guid userId, ChangeEmailDto changeEmailDto, CancellationToken cancellationToken = default);
    }
}

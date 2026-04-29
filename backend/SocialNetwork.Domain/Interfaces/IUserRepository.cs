using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IUserRepository : IGenerycRepository<User>
    {
        public Task<User?> GetByEmailAsync(string Email, CancellationToken cancellationToken = default);
        public Task<User?> GetByUserNameAsync(string UserName, CancellationToken cancellationToken = default);
        public Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
        public Task<(IEnumerable<User> Items, int Total)> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}

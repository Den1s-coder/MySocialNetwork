using SocialNetwork.Domain.Entities.Users;
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

    }
}

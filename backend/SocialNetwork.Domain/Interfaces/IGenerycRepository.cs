using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IGenerycRepository<T> where T : class
    {
        public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task CreateAsync(T T, CancellationToken cancellationToken = default);
        public Task UpdateAsync(T T, CancellationToken cancellationToken = default);
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

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
        public Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdAsync(Guid id);
        public Task CreateAsync(T T);
        public Task UpdateAsync(T T);
        public Task DeleteAsync(Guid id);
    }
}

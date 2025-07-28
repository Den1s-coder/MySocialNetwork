using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IPostRepository
    {
        public Task<IEnumerable<Post>> GetAll();
        public Task<Post?> GetById(Guid id);
        public Task CreateAsync(Post post);
        public Task UpdateAsync(Post post);
        public Task DeleteAsync(Guid id);
    }
}

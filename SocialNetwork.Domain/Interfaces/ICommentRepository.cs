using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface ICommentRepository
    {
        public Task CreateAsync(Comment comment);
        public Task<Comment?> GetById(Guid id);
        public Task<IEnumerable<Comment>> GetAll();
        public Task UpdateAsync(Comment comment);
        public Task DeleteAsync(Guid id);
    }
}

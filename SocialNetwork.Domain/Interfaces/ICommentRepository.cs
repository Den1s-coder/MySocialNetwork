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
        public Task<Comment?> GetById(Guid id);
        public Task<IEnumerable<Comment>> GetAll();
        public Task Update(Comment comment);
        public Task Delete(Guid id);
    }
}

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
        public Task<Comment> GetById(int id);
        public Task<List<Comment>> GetAll();
        public Task<Comment> Update(Comment comment);
        public Task Delete(int id);
    }
}

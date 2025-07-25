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
        public Comment GetById(int id);
        public List<Comment> GetAll();
        public Comment Update(Comment comment);
        public void Delete(int id);
    }
}

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
        public Task<Post> GetById(int id);
        public Task<Post> Create(Post post);
        public Task<Post> Update(Post post);
        public Task DeleteById(int id);
    }
}

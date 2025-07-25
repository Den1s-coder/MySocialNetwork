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
        public Post GetById(int id);
        public Post Create(Post post);
        public Post Update(Post post);
        public void DeleteById(int id);
    }
}

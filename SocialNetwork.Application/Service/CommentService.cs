using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class CommentService : ICommentService
    {
        public Task BanComment(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(CreateCommentDto commentDto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Comment>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Comment?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

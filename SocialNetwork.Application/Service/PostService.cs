using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class PostService : IPostService
    {
        public Task BanPost(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(CreatePostDto postDto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Post?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByTagAsync(string tag)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}

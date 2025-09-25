using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetAllAsync();
        Task<PostDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreatePostDto postDto);
        Task BanPost(Guid id);
        Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(Guid userId);
        Task<IEnumerable<PostDto>> GetPostsByTagAsync(string tag);
        Task<IEnumerable<PostDto>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

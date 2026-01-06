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
        Task<IEnumerable<PostDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(CreatePostDto postDto, CancellationToken cancellationToken = default);
        Task BanPost(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostDto>> GetPostsByTagAsync(string tag, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostDto>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<PaginetedResult<PostDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}

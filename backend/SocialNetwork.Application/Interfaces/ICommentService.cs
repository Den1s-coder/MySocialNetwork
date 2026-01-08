using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface ICommentService
    {
        public Task<IEnumerable<CommentDto>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task<CommentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task CreateAsync(CreateCommentDto commentDto, CancellationToken cancellationToken = default);
        public Task BanComment(Guid id, CancellationToken cancellationToken = default);
        public Task<PaginetedResult<CommentDto>> GetPostCommentsPagedAsync(Guid id,int page,int pageSize, CancellationToken cancellationToken = default);

    }
}

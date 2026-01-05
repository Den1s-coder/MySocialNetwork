using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface ICommentService
    {
        public Task<IEnumerable<CommentDto>> GetAllAsync(CancellationToken cancellationToken = default);
        public Task<CommentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task CreateAsync(CreateCommentDto commentDto, CancellationToken cancellationToken = default);
        public Task BanComment(Guid id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<CommentDto>> GetPostCommentsAsync(Guid id, CancellationToken cancellationToken = default);

    }
}

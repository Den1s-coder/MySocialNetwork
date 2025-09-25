using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface ICommentService
    {
        public Task<IEnumerable<CommentDto>> GetAllAsync();
        public Task<CommentDto?> GetByIdAsync(Guid id);
        public Task CreateAsync(CreateCommentDto commentDto);
        public Task BanComment(Guid id);
        public Task<IEnumerable<CommentDto>> GetPostCommentsAsync(Guid id);

    }
}

using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface ICommentService
    {
        public Task<IEnumerable<Comment>> GetAllAsync();
        public Task<Comment?> GetByIdAsync(Guid id);
        public Task CreateAsync(CreateCommentDto commentDto);
        public Task BanComment(Guid id);
        public Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid id);

    }
}

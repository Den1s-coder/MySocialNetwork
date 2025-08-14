using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Domain.Interfaces
{
    public interface ICommentService
    {
        public Task<IEnumerable<Comment>> GetAllAsync();
        public Task<Comment?> GetByIdAsync(Guid id);
        public Task CreateAsync(CreateCommentDto commentDto);
        public Task BanComment(Guid id);

    }
}

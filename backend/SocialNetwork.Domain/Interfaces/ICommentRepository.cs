using SocialNetwork.Domain.Entities.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface ICommentRepository: IGenerycRepository<Comment>
    {
        public Task<(IEnumerable<Comment> Items, int Total)> GetPostCommentsPagedAsync(Guid id,int page,int pageSize, CancellationToken cancellationToken = default);
        public Task ToggleReactionAsync(Guid commentId, Guid userId, Guid reactionType, CancellationToken cancellationToken = default);
    }
}

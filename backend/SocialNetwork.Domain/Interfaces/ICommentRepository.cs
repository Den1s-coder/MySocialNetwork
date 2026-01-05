using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface ICommentRepository: IGenerycRepository<Comment>
    {
        public Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IPostRepository: IGenerycRepository<Post>
    {
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId);
        Task<IEnumerable<Post>> GetPostsByTagAsync(string tag);
        Task<IEnumerable<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

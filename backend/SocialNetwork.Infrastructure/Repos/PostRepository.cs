using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Repos
{
    public class PostRepository : IPostRepository
    {
        private SocialDbContext _context;

        public PostRepository(SocialDbContext context) 
        {
            _context = context;
        }

        public async Task CreateAsync(Post post, CancellationToken cancellationToken = default)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .Include(p => p.User)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Post> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<IEnumerable<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default) //TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByTagAsync(string tag, CancellationToken cancellationToken = default) //TODO
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.Comments)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task UpdateAsync(Post updatedPost, CancellationToken cancellationToken = default)
        {
            var existingPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == updatedPost.Id);

            if (existingPost == null) 
                throw new ArgumentException("Post not found");

            existingPost.Text = updatedPost.Text;
            existingPost.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

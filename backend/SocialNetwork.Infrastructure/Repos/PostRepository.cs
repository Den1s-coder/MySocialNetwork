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

        public async Task CreateAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<IEnumerable<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate) //TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByTagAsync(string tag) //TODO
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.Comments)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task UpdateAsync(Post updatedPost)
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

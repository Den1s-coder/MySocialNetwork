using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Repos
{
    public class CommentRepository : ICommentRepository
    {
        private SocialDbContext _context { get; set; }

        public CommentRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            if(comment == null) return;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid id)
        {
            return await _context.Comments
                .Where(c => c.PostId == id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(Guid id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Comment updatedComment)
        {
            var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == updatedComment.Id);

            if(existingComment == null) return;

            existingComment.Text = updatedComment.Text;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
    }
}

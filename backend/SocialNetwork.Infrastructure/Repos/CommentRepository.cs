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

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            if(comment == null) return;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(IEnumerable<Comment> Items, int Total)> GetPostCommentsPagedAsync(Guid id,int page,int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Comments
                .AsNoTracking()
                .Where(c => c.PostId == id)
                .Include(c => c.Author)
                .OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Comment updatedComment, CancellationToken cancellationToken = default)
        {
            var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == updatedComment.Id);

            if(existingComment == null) return;

            existingComment.Text = updatedComment.Text;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CreateAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
    }
}

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
    internal class CommentRepository : ICommentRepository
    {
        private SocialDbContext _context { get; set; }

        public CommentRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task Delete(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            if(comment == null) return;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetAll()
        {
            return await _context.Comments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comment?> GetById(Guid id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task Update(Comment updatedComment)
        {
            var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == updatedComment.Id);

            if(existingComment == null) return;

            existingComment.Text = updatedComment.Text;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}

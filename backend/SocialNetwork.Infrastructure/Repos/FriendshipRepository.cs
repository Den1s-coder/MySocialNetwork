using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Repos
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly SocialDbContext _context;

        public FriendshipRepository(SocialDbContext context)
        {
            _context = context;
        }

        public Task CreateAsync(Friendship T, CancellationToken cancellationToken = default)
        {
            _context.Friendships.Add(T);
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<Friendship>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Friendships
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Friendships
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Friendship T, CancellationToken cancellationToken = default)
        {
            _context.Friendships.Update(T);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existFriendship = await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (existFriendship == null) return;
            
            _context.Friendships.Remove(existFriendship);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
        {
            return _context.Friendships
                .AsNoTracking()
                .AnyAsync(f => (f.RequesterId == userId1 && f.AddresseeId == userId2) ||
                        (f.AddresseeId == userId2 && f.RequesterId == userId1), cancellationToken);
        }
    }
}

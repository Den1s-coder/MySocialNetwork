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
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly SocialDbContext _context;

        public FriendshipRepository(SocialDbContext context)
        {
            _context = context;
        }

        public Task CreateAsync(Friendship T)
        {
            _context.Friendships.Add(T);
            return _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Friendship>> GetAllAsync()
        {
            return await _context.Friendships
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Friendship?> GetByIdAsync(Guid id)
        {
            return await _context.Friendships
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(Guid userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
                .ToListAsync();
        }

        public Task UpdateAsync(Friendship T) //TODO
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existFriendship = await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id);

            if (existFriendship == null) return;
            
            _context.Friendships.Remove(existFriendship);
            await _context.SaveChangesAsync();
        }

        public Task<bool> AreFriendsAsync(Guid userId1, Guid userId2)
        {
            return _context.Friendships
                .AsNoTracking()
                .AnyAsync(f => (f.RequesterId == userId1 && f.AddresseeId == userId2) ||
                               (f.AddresseeId == userId2 && f.RequesterId == userId1));
        }
    }
}

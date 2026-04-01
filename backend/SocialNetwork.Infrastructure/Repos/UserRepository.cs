using SocialNetwork.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialNetwork.Domain.Entities.Users;
using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Infrastructure.Repos
{
    public class UserRepository : IUserRepository
    {
        private SocialDbContext _context;

        public UserRepository(SocialDbContext context) 
        {
            _context = context;
        }

        public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null) return;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == userName, cancellationToken);
        }

        public async Task UpdateAsync(User updatedUser, CancellationToken cancellationToken = default)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == updatedUser.Id, cancellationToken);

            if (existingUser == null) return;

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.ProfilePictureUrl = updatedUser.ProfilePictureUrl;
            existingUser.IsBanned = updatedUser.IsBanned;

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                existingUser.PasswordHash = updatedUser.PasswordHash;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == role)
                .ToListAsync(cancellationToken);
        }
    }
}

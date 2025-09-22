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
    public class UserRepository : IUserRepository
    {
        private SocialDbContext _context;

        public UserRepository(SocialDbContext context) 
        {
            _context = context;
        }

        public async Task CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<User?> GetByUserNameAsync(string UserName)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Name == UserName);
        }

        public async Task UpdateAsync(User updatedUser)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == updatedUser.Id);

            if (existingUser == null) return;

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            await _context.SaveChangesAsync();
        }
    }
}

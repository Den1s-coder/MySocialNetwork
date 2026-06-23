using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Infrastructure.Repos
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly SocialDbContext _context;
        public RefreshTokenRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == refreshToken.Id);

            if (existingToken == null) return;

            existingToken.Token = refreshToken.Token;
            existingToken.ExpiresOn = refreshToken.ExpiresOn;

            await _context.SaveChangesAsync();
        }

        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
    }
}

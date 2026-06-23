using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
    }
}
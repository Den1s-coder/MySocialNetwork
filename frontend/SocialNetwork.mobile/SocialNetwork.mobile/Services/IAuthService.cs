using System.Threading.Tasks;

namespace SocialNetwork.mobile.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task RegisterAsync(string username, string email, string password);
        Task LogoutAsync();
        Task<string> GetTokenAsync();
    }
}

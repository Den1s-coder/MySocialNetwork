using SocialNetwork.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IAuthService
    {
        public Task RegisterAsync(RegisterDto userDto, CancellationToken cancellationToken = default);
        public Task<LoginResponce> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        public Task<LoginResponce> LoginWithRefreshTokenAsync(string refreshToken);
        public Task<LoginResponce> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default);
    }
}

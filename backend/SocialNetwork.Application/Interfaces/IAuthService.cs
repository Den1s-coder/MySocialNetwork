using SocialNetwork.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Interfaces
{
    public interface IAuthService
    {
        public Task RegisterAsync(RegisterDto userDto);
        public Task<LoginResponce> LoginAsync(LoginDto loginDto);
        public Task<LoginResponce> LoginWithRefreshTokenAsync(string refreshToken);
    }
}

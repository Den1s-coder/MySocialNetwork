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
        public Task<string> LoginAsync(LoginDto loginDto);
    }
}

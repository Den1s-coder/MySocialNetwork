using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Auth
{
    public record LoginResponce
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public LoginResponce(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}

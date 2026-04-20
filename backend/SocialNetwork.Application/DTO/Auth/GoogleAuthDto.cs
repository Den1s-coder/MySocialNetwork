using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Auth
{
    public record GoogleAuthDto
    {
        public string IdToken { get; init; }
    }
}

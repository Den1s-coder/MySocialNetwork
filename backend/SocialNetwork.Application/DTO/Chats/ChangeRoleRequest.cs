using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Chats
{
    public record ChangeRoleRequest
    {
        public int NewRole { get; init; }
    }
}

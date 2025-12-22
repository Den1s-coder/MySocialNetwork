using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public record FriendRequestDto
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid RequesterId { get; init; }
        public Guid ReceiverId { get; init; }
        public DateTime RequestedAt { get; init; }
    }
}

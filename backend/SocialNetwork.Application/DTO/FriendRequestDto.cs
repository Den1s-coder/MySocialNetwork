using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public record FriendRequestDto
    {
        public Guid Id { get; init; }
        public Guid RequesterId { get; init; }
        public Guid ReceiverId { get; init; }
        public string RequesterUsername { get; init; }
        public DateTime RequestedAt { get; init; }
    }
}

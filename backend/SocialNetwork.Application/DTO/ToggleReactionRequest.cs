using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public record ToggleReactionRequest
    {
        public Guid ReactionTypeId { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public record ReactionSummaryDto
    {
        public string Code { get; init; }
        public string Symbol { get; init; }
        public int Count { get; init; }
        public bool IsReactedByCurrentUser { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities.Reactions
{
    public class ReactionType: BaseEntity
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }
}

using SocialNetwork.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Entities.Reactions
{
    public abstract class BaseReaction: BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ReactionTypeId { get; set; }
        public ReactionType ReactionType { get; set; }
    }
}

using SocialNetwork.Domain.Entities.Reactions;

namespace SocialNetwork.Domain.Entities.Posts
{
    public class PostReaction : BaseReaction
    {
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        public PostReaction() { } //for EF migrations
        public PostReaction(Guid userId, Guid postId, Guid reactionTypeId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PostId = postId;
            ReactionTypeId = reactionTypeId;

        }
    }
}

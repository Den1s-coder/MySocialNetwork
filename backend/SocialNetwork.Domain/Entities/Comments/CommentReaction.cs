using SocialNetwork.Domain.Entities.Reactions;

namespace SocialNetwork.Domain.Entities.Comments
{
    public class CommentReaction: BaseReaction
    {
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; }
        public CommentReaction() { } //for EF migrations
        public CommentReaction(Guid userId, Guid commentId, Guid reactionTypeId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CommentId = commentId;
            ReactionTypeId = reactionTypeId;
        }
    }
}

using SocialNetwork.Domain.Entities.Reactions;

namespace SocialNetwork.Domain.Entities.Chats
{
    public class MessageReaction: BaseReaction
    {
        public Guid MessageId { get; set; }
        public Message Message { get; set; }
        public MessageReaction() { } //for EF migrations
        public MessageReaction(Guid userId, Guid messageId, Guid reactionTypeId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            MessageId = messageId;
            ReactionTypeId = reactionTypeId;
        }
    }
}

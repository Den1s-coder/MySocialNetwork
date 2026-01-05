using SocialNetwork.Application.DTO.Chats;

namespace SocialNetwork.Application.Interfaces
{
    public interface IMessageService
    {
        public Task<IEnumerable<Message>> GetMessageByChatIdAsync(Guid chatid, CancellationToken cancellationToken = default);
    }
}

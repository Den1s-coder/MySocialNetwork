using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface IMessageService
    {
        public Task<IEnumerable<Message>> GetMessageByChatIdAsync(Guid chatid);
    }
}

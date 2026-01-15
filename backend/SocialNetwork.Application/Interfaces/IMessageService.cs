using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Interfaces
{
    public interface IMessageService
    {
        public Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatid, CancellationToken cancellationToken = default);
    }
}

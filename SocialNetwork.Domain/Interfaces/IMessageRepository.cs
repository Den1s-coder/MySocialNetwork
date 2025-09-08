using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Domain.Interfaces
{
    public interface IMessageRepository: IGenerycRepository<Message>
    {
        public Task<IEnumerable<Message>> GetMessagesByChatIdAsync(Guid chatId);
        public Task<IEnumerable<Message>> GetMessagesBySenderIdAsync(Guid senderId);
        public Task<IEnumerable<Message>> SearchMessagesInChatAsync(Guid chatId, string searchTerm);
    }
}

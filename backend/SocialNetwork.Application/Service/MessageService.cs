using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class MessageService: IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<Message>> GetMessageByChatIdAsync(Guid chatid)
        {
            return await _messageRepository.GetMessagesByChatIdAsync(chatid);
        }
    }
}

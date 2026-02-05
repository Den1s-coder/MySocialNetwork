using AutoMapper;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class MessageService: IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessageService(IMessageRepository messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDto>> GetMessageByChatIdAsync(Guid chatid, CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetMessagesByChatIdAsync(chatid);

            return _mapper.Map<IEnumerable<Message>, IEnumerable<MessageDto>>(messages);
        }
    }
}

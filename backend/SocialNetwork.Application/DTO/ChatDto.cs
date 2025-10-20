using SocialNetwork.Domain.Enums;

namespace SocialNetwork.Application.DTO
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public ChatType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserChatDto> UserChats { get; set; } = new List<UserChatDto>();
    }
}

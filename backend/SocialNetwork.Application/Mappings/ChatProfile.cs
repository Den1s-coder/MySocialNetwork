using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;


namespace SocialNetwork.Application.Mappings
{
    public class ChatProfile: Profile
    {
        public ChatProfile()
        {
           CreateMap<Chat, ChatDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ReverseMap();

            CreateMap<UserChat, UserChatDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => src.ChatId))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Rights, opt => opt.MapFrom(src => src.Rights))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePictureUrl : null))
                .ReverseMap();
        }
    }
}

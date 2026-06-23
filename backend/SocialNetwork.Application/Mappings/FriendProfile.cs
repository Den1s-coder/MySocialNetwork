using AutoMapper;
using SocialNetwork.Application.DTO.Users;
using SocialNetwork.Domain.Entities.Users;

namespace SocialNetwork.Application.Mappings
{
    public class FriendProfile : Profile
    {
        public FriendProfile()
        {
            CreateMap<Friendship, FriendRequestDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RequesterId, opt => opt.MapFrom(src => src.RequesterId))
                .ForMember(dest => dest.ReceiverId, opt => opt.MapFrom(src => src.AddresseeId))
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ReverseMap();

            CreateMap<User, UserDto>()
                .ReverseMap();
        }
    }
}

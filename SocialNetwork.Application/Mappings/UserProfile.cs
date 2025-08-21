using AutoMapper;

namespace SocialNetwork.Application.Mappings
{
    public class UserProfile:Profile
    {
        public UserProfile() 
        {
            CreateMap<Domain.Entities.User, DTO.CreateUserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ReverseMap();
        }
    }
}

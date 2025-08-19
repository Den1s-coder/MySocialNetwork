using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Mappings
{
    public class PostProfile: Profile
    {
        public PostProfile()
        { 
            CreateMap<Post, CreatePostDto>()
                 .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                 .ReverseMap();
        }
    }
}

using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Mappings
{
    public class CommentProfile: Profile
    {
        public CommentProfile()
        {
            CreateMap<Domain.Entities.Comment, DTO.CreateCommentDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
                .ReverseMap();
        }
    }
}

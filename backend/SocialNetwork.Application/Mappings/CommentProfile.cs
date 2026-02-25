using AutoMapper;
using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Domain.Entities.Comments;
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
            CreateMap<Comment, CreateCommentDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
                .ReverseMap();

            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Author.Name))
                .ForMember(dest => dest.IsBanned, opt => opt.MapFrom(src => src.IsBanned))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Author.ProfilePictureUrl))
                .ReverseMap();
        }
    }
}

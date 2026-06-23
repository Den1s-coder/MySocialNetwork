using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Chats;
using SocialNetwork.Domain.Entities.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Mappings
{
    public class MessageProfile: Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => src.ChatId))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.Name))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.Sender.ProfilePictureUrl))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt))
                .ForMember(dest => dest.Reactions, opt => opt.MapFrom(src => 
                src.Reactions
                .GroupBy(r => r.ReactionType.Code)
                .Select(r => new ReactionSummaryDto
                {
                    Code = r.Key,
                    Symbol = r.First().ReactionType.Symbol,
                    Count = r.Count()
                })
                .OrderBy(r => r.Code)
                .ToList()))
                .ForMember(dest => dest.CurrentUserReactionCode, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}

using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.DTO.Posts;
using SocialNetwork.Domain.Entities.Posts;

namespace SocialNetwork.Application.Mappings
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, CreatePostDto>()
                 .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                 .ReverseMap();

            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.IsBanned, opt => opt.MapFrom(src => src.IsBanned))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
                .ForMember(dest => dest.Reactions, opt => opt.MapFrom((src, dest, _, context) =>
                {
                    if (src.Reactions == null || src.Reactions.Count == 0)
                        return new List<ReactionSummaryDto>();

                    context.Items.TryGetValue("CurrentUserId", out var rawUserId);
                    var currentUserId = rawUserId as Guid?;

                    return src.Reactions
                        .GroupBy(r => new { r.ReactionType.Code, r.ReactionType.Symbol })
                        .OrderBy(g => g.First().ReactionType.SortOrder)
                        .Select(g => new ReactionSummaryDto
                        {
                            Code = g.Key.Code,
                            Symbol = g.Key.Symbol,
                            Count = g.Count(),
                            IsReactedByCurrentUser = currentUserId.HasValue
                                && g.Any(r => r.UserId == currentUserId.Value)
                        })
                        .ToList();
                }))
                .ReverseMap();
        }
    }
}

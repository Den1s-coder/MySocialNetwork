using SocialNetwork.Application.DTO.Comments;
using SocialNetwork.Domain.Entities.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Posts
{
    public record PostDto
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; }
        public string? ImageUrl { get; set; }
        public string UserName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public List<CommentDto> Comments { get; set; }
        public List<ReactionSummaryDto> Reactions { get; set; }
        public string? CurrentUserReactionCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsBanned { get; set; }

    }
}

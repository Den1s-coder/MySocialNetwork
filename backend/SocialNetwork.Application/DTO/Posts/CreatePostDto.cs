using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Posts
{
    public record CreatePostDto
    {
        [Required]
        public string Text { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}

using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO.Comments
{
    public record CreateCommentDto
    {
        [Required] 
        public string Text { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }
        [Required]
        public Guid PostId { get; set; }    
    }
}

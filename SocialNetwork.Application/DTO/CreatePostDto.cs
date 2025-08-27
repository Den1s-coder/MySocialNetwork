using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public class CreatePostDto
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}

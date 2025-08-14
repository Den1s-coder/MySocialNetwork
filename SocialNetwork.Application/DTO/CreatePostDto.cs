using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.DTO
{
    public class CreatePostDto
    {
        public string Text { get; set; }
        public Guid UserId { get; set; }
    }
}

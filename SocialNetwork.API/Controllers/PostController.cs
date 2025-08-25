using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet]
        public Task<IActionResult> GetAllPosts()
        {
            return ;
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetById(int id)
        {
            return "value";
        }

        [HttpPost]
        public Task<IActionResult> Post([FromBody] string value)
        {
        }

        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id)
        {
        }
    }
}

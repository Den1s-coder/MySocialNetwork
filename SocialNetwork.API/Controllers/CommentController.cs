using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        [HttpGet]
        public Task<IActionResult> Get()
        {
            
        }

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id)
        {
            
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConcordCloud.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var role = User.FindFirstValue("role");

            return Ok(new
            {
                message = "认证成功！",
                userId,
                email,
                role,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("public")]
        public IActionResult TestPublic()
        {
            return Ok(new { message = "这是一个公开的API，无需认证" });
        }
    }
}
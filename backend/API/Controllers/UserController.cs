using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcordCloud.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _userService.RegisterAsync(registerDto);
            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message, user });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _userService.LoginAsync(loginDto);
            if (!success)
            {
                return BadRequest(new { success, message });
            }

            // 创建身份验证Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            };

            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new { success, message, user });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return Ok(new { success = true, message = "已成功退出登录" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // 考虑使用 TryParse 以增加健壮性
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
           
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "用户不存在" });
            }

            return Ok(new { success = true, user });
        }
    }
}
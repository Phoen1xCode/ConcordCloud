using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcordCloud.API.Controllers;

[ApiController]
[Route("api/user")]
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
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        var (success, message, user) = await _userService.RegisterAsync(registerDto);
        if (!success)
        {
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Created(user, message).ToActionResult();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        var (success, message, user) = await _userService.LoginAsync(loginDto);
        if (!success)
        {
            return ApiResponse<object>.Unauthorized(message).ToActionResult();
        }

        // Create authentication cookie
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

        return ApiResponse<object>.Ok(user, message).ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return ApiResponse<object>.Ok("Logout successful").ToActionResult();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        // Consider using TryParse to increase robustness
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<object>.NotFound("User does not exist").ToActionResult();
        }

        return ApiResponse<object>.Ok(user).ToActionResult();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] UserUpdatePasswordDto passwordDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }

        var (success, message) = await _userService.ChangePasswordAsync(userId, passwordDto);
        if (!success)
        {
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }
}
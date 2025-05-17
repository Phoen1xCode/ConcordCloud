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
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        var (success, message, admin) = await _adminService.LoginAsync(loginDto);
        if (!success || admin == null)
        {
            return ApiResponse<object>.Unauthorized(message).ToActionResult();
        }

        // Create admin authentication cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
        };

        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        return ApiResponse<object>.Ok(admin, message).ToActionResult();
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return ApiResponse<object>.Ok("Logout successful").ToActionResult();
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProfile()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
        {
            return ApiResponse<object>.BadRequest("Invalid admin identifier").ToActionResult();
        }

        var admin = await _adminService.GetAdminByIdAsync(adminId);
        if (admin == null)
        {
            return ApiResponse<object>.NotFound("Admin account does not exist").ToActionResult();
        }

        return ApiResponse<object>.Ok(admin).ToActionResult();
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword([FromBody] AdminUpdatePasswordDto passwordDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
        {
            return ApiResponse<object>.BadRequest("Invalid admin identifier").ToActionResult();
        }

        var (success, message) = await _adminService.ChangePasswordAsync(adminId, passwordDto);
        if (!success)
        {
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return ApiResponse<object>.Ok(users).ToActionResult();
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserDetails(Guid userId)
    {
        var user = await _adminService.GetUserDetailsAsync(userId);
        if (user == null)
        {
            return ApiResponse<object>.NotFound("User does not exist").ToActionResult();
        }

        return ApiResponse<object>.Ok(user).ToActionResult();
    }

    [HttpPost("users/{userId}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetUserPassword(Guid userId, [FromBody] AdminResetUserPasswordDto resetDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        var (success, message) = await _adminService.ResetUserPasswordAsync(userId, resetDto);
        if (!success)
        {
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }

    [HttpDelete("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var (success, message) = await _adminService.DeleteUserAsync(userId);
        if (!success)
        {
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }

    [HttpGet("files")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFiles()
    {
        var files = await _adminService.GetAllFilesAsync();
        return ApiResponse<object>.Ok(files).ToActionResult();
    }

    [HttpGet("users/{userId}/files")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserFiles(Guid userId)
    {
        var files = await _adminService.GetUserFilesAsync(userId);
        return ApiResponse<object>.Ok(files).ToActionResult();
    }

    [HttpDelete("files/{fileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        var (success, message) = await _adminService.DeleteFileAsync(fileId);
        if (!success)
        {
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPlatformStatistics()
    {
        var statistics = await _adminService.GetPlatformStatisticsAsync();
        return ApiResponse<object>.Ok(statistics).ToActionResult();
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeAdmin([FromBody] AdminCreateDto adminDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        var (success, message) = await _adminService.InitializeDefaultAdminAsync(adminDto);
        if (!success)
        {
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        return ApiResponse<object>.Ok(message).ToActionResult();
    }
} 
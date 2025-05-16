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
            return BadRequest(ApiResponse.Error("无效请求数据"));
        }

        var (success, message, admin) = await _adminService.LoginAsync(loginDto);
        if (!success || admin == null)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        // 创建管理员身份认证 Cookie
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

        return Ok(ApiResponse.Ok(admin, message));
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return Ok(ApiResponse.Ok("退出登录成功"));
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProfile()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
        {
            return BadRequest(ApiResponse.Error("无效的管理员标识"));
        }

        var admin = await _adminService.GetAdminByIdAsync(adminId);
        if (admin == null)
        {
            return NotFound(ApiResponse.Error("管理员账号不存在"));
        }

        return Ok(ApiResponse.Ok(admin));
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword([FromBody] AdminUpdatePasswordDto passwordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Error("无效请求数据"));
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
        {
            return BadRequest(ApiResponse.Error("无效的管理员标识"));
        }

        var (success, message) = await _adminService.ChangePasswordAsync(adminId, passwordDto);
        if (!success)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(ApiResponse.Ok(users));
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserDetails(Guid userId)
    {
        var user = await _adminService.GetUserDetailsAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse.Error("用户不存在"));
        }

        return Ok(ApiResponse.Ok(user));
    }

    [HttpPost("users/{userId}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetUserPassword(Guid userId, [FromBody] AdminResetUserPasswordDto resetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Error("无效请求数据"));
        }

        var (success, message) = await _adminService.ResetUserPasswordAsync(userId, resetDto);
        if (!success)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var (success, message) = await _adminService.DeleteUserAsync(userId);
        if (!success)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("files")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFiles()
    {
        var files = await _adminService.GetAllFilesAsync();
        return Ok(ApiResponse.Ok(files));
    }

    [HttpGet("users/{userId}/files")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserFiles(Guid userId)
    {
        var files = await _adminService.GetUserFilesAsync(userId);
        return Ok(ApiResponse.Ok(files));
    }

    [HttpDelete("files/{fileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        var (success, message) = await _adminService.DeleteFileAsync(fileId);
        if (!success)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPlatformStatistics()
    {
        var statistics = await _adminService.GetPlatformStatisticsAsync();
        return Ok(ApiResponse.Ok(statistics));
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeAdmin([FromBody] AdminCreateDto adminDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Error("无效请求数据"));
        }

        var (success, message) = await _adminService.InitializeDefaultAdminAsync(adminDto);
        if (!success)
        {
            return BadRequest(ApiResponse.Error(message));
        }

        return Ok(ApiResponse.Ok(message));
    }
} 
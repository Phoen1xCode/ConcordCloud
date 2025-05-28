using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConcordCloud.API.Controllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    // Helper method to get user ID safely
    private bool TryGetUserId(out Guid userId)
    {
        if (User == null)
        {
            userId = Guid.Empty;
            return false;
        }
        
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserFiles()
    {
        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }
        var files = await _fileService.GetUserFilesAsync(userId);
        return ApiResponse<object>.Ok(files).ToActionResult();
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ApiResponse<object>.BadRequest("Please select a file to upload").ToActionResult();
        }

        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }

        using var stream = file.OpenReadStream();
        var result = await _fileService.UploadFileAsync(
            userId,
            file.FileName, 
            file.ContentType,
            file.Length,
            stream);

        if (!result.Success)
        {
            return ApiResponse<object>.BadRequest(result.Message ?? "Upload failed").ToActionResult();
        }

        if (result.File == null)
        {
            return ApiResponse<object>.BadRequest("File upload failed: no file data returned").ToActionResult();
        }

        return ApiResponse<object>.Created(result.File, result.Message ?? "File uploaded successfully").ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }
        var (success, message) = await _fileService.DeleteFileAsync(userId, id);

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

    [HttpPut("rename")]
    [Authorize]
    public async Task<IActionResult> RenameFile([FromBody] FileRenameDto renameDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }
        
        var (success, message, file) = await _fileService.RenameFileAsync(userId, renameDto);

        if (!success)
        {
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        if (file == null)
        {
            return ApiResponse<object>.BadRequest("File data is null").ToActionResult();
        }

        return ApiResponse<object>.Ok(file, message).ToActionResult();
    }

    [HttpPost("share")]
    [Authorize]
    public async Task<IActionResult> CreateFileShare([FromBody] FileShareDto shareDto)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponse<object>.BadRequest("Invalid request data").ToActionResult();
        }

        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }
        var result = await _fileService.CreateFileShareAsync(userId, shareDto);

        if (result == null)
        {
            return ApiResponse<object>.NotFound("File not found or no permission to share").ToActionResult();
        }

        return ApiResponse<object>.Created(result, "Share created successfully").ToActionResult();
    }

    [HttpGet("download/{id}")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return ApiResponse<object>.BadRequest("Invalid user identifier").ToActionResult();
        }
        var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByIdAsync(userId, id);

        if (!success)
        {
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        if (fileStream == null)
        {
            return ApiResponse<object>.BadRequest("File stream is null").ToActionResult();
        }

        return File(fileStream, contentType ?? "application/octet-stream", fileName);
    }

    [HttpGet("shared/{shareCode}")]
    public async Task<IActionResult> DownloadSharedFile(string shareCode)
    {
        if (string.IsNullOrWhiteSpace(shareCode))
        {
            return ApiResponse<object>.BadRequest("Share code cannot be empty").ToActionResult();
        }

        var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByShareCodeAsync(shareCode);

        if (!success)
        {
            if (message.Contains("expired", StringComparison.OrdinalIgnoreCase) || 
                message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<object>.NotFound(message).ToActionResult();
            }
            return ApiResponse<object>.BadRequest(message).ToActionResult();
        }

        if (fileStream == null)
        {
            return ApiResponse<object>.BadRequest("File stream is null").ToActionResult();
        }

        return File(fileStream, contentType ?? "application/octet-stream", fileName);
    }
}
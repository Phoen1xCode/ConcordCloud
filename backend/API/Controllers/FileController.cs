using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConcordCloud.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserFiles()
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }
            var files = await _fileService.GetUserFilesAsync(userId);
            return Ok(ApiResponse.Ok(files));
        }

        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(100 * 1024 * 1024)] // Example: Limit uploads to 100 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse.Error("Please select a file to upload"));
            }
            // Add file size check here if needed, although RequestSizeLimit helps
            // if (file.Length > YOUR_MAX_SIZE_IN_BYTES) { ... }

            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }

            using (var stream = file.OpenReadStream())
            {
                var result = await _fileService.UploadFileAsync(
                    userId,
                    file.FileName, // Service layer should handle sanitization if needed
                    file.ContentType,
                    file.Length,
                    stream);

                if (!result.Success)
                {
                    // Consider logging the error message here
                    return BadRequest(ApiResponse.Error(result.Message));
                }

                return Ok(ApiResponse.Ok(result.File, result.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }
            var (success, message) = await _fileService.DeleteFileAsync(userId, id);

            if (!success)
            {
                // Consider returning NotFound if the message indicates file not found vs. permission issue
                return BadRequest(ApiResponse.Error(message));
            }

            return Ok(ApiResponse.Ok(message));
        }

        [HttpPut("rename")]
        [Authorize]
        public async Task<IActionResult> RenameFile([FromBody] FileRenameDto renameDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Error("Invalid request data"));
            }

            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }
            var (success, message, file) = await _fileService.RenameFileAsync(userId, renameDto);

            if (!success)
            {
                return BadRequest(ApiResponse.Error(message));
            }

            return Ok(ApiResponse.Ok(file, message));
        }

        [HttpPost("share")]
        [Authorize]
        public async Task<IActionResult> CreateFileShare([FromBody] FileShareDto shareDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Error("Invalid request data"));
            }

            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }
            var result = await _fileService.CreateFileShareAsync(userId, shareDto);

            if (result == null) // Assuming null indicates failure (e.g., file not found or permission denied)
            {
                return BadRequest(ApiResponse.Error("Failed to create share"));
            }

            return Ok(ApiResponse.Ok(result, "Share created successfully"));
        }

        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(ApiResponse.Error("Invalid user identifier"));
            }
            var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByIdAsync(userId, id);

            if (!success)
            {
                // Consider returning NotFound if appropriate
                return BadRequest(ApiResponse.Error(message));
            }

            // IMPORTANT: Ensure the fileStream is disposed correctly after the response is sent.
            // FileStreamResult handles this automatically.
            return File(fileStream, contentType, fileName);
        }

        [HttpGet("shared/{shareCode}")]
        public async Task<IActionResult> DownloadSharedFile(string shareCode)
        {
            if (string.IsNullOrWhiteSpace(shareCode))
            {
                return BadRequest(ApiResponse.Error("Share code cannot be empty"));
            }

            var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByShareCodeAsync(shareCode);

            if (!success)
            {
                // Consider returning NotFound or Gone if the link expired/invalid
                return BadRequest(ApiResponse.Error(message));
            }

            return File(fileStream, contentType, fileName);
        }
    }
}
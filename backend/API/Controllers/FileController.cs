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
                return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
            var files = await _fileService.GetUserFilesAsync(userId);
            return Ok(new { success = true, files });
        }

        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(100 * 1024 * 1024)] // Example: Limit uploads to 100 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "请选择要上传的文件" });
            }
             // Add file size check here if needed, although RequestSizeLimit helps
            // if (file.Length > YOUR_MAX_SIZE_IN_BYTES) { ... }

            if (!TryGetUserId(out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
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
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message, file = result.File });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
             if (!TryGetUserId(out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
            var (success, message) = await _fileService.DeleteFileAsync(userId, id);

            if (!success)
            {
                 // Consider returning NotFound if the message indicates file not found vs. permission issue
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        [HttpPut("rename")]
        [Authorize]
        public async Task<IActionResult> RenameFile([FromBody] FileRenameDto renameDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryGetUserId(out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
            var (success, message, file) = await _fileService.RenameFileAsync(userId, renameDto);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message, file });
        }

        [HttpPost("share")]
        [Authorize]
        public async Task<IActionResult> CreateFileShare([FromBody] FileShareDto shareDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

             if (!TryGetUserId(out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
            var result = await _fileService.CreateFileShareAsync(userId, shareDto);

            if (result == null) // Assuming null indicates failure (e.g., file not found or permission denied)
            {
                return BadRequest(new { success = false, message = "创建分享失败" });
            }

            return Ok(new { success = true, message = "创建分享成功", share = result });
        }

        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
             if (!TryGetUserId(out var userId))
            {
                 return BadRequest(new { success = false, message = "无效的用户标识符" });
            }
            var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByIdAsync(userId, id);

            if (!success)
            {
                // Consider returning NotFound if appropriate
                return BadRequest(new { success, message });
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
                 return BadRequest(new { success = false, message = "分享码不能为空" });
             }

            var (success, message, fileStream, fileName, contentType) = await _fileService.DownloadFileByShareCodeAsync(shareCode);

            if (!success)
            {
                 // Consider returning NotFound or Gone if the link expired/invalid
                return BadRequest(new { success, message });
            }

            return File(fileStream, contentType, fileName);
        }
    }
}
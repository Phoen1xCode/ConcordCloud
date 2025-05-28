using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;

namespace ConcordCloud.Core.Interfaces;

public interface IFileService
{
    Task<FileUploadResultDto> UploadFileAsync(Guid userId, string originalFileName, string contentType, long fileSize, Stream fileStream);
    Task<IEnumerable<FileDto>> GetUserFilesAsync(Guid userId);
    Task<(bool Success, string Message)> DeleteFileAsync(Guid userId, Guid fileId);
    Task<(bool Success, string Message, FileDto? File)> RenameFileAsync(Guid userId, FileRenameDto renameDto);
    Task<FileShareResultDto> CreateFileShareAsync(Guid userId, FileShareDto shareDto);
    Task<(bool Success, string Message, Stream? FileStream, string? FileName, string? ContentType)> DownloadFileByIdAsync(Guid userId, Guid fileId);
    Task<(bool Success, string Message, Stream? FileStream, string? FileName, string? ContentType)> DownloadFileByShareCodeAsync(string shareCode);
} 
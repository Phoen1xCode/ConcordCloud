using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConcordCloud.Core.Services;

/// <summary>
/// Service responsible for handling file operations including upload, download, sharing, and management
/// </summary>
public class FileService : IFileService
{
    private readonly IAppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Initializes a new instance of the FileService
    /// </summary>
    /// <param name="dbContext">Database context for file operations</param>
    /// <param name="fileStorageService">Service for handling physical file storage</param>
    public FileService(IAppDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Uploads a file for a specific user
    /// </summary>
    /// <param name="userId">ID of the user uploading the file</param>
    /// <param name="originalFileName">Original name of the file</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="fileSize">Size of the file in bytes</param>
    /// <param name="fileStream">Stream containing the file data</param>
    /// <returns>Result of the upload operation including file details</returns>
    public async Task<FileUploadResultDto> UploadFileAsync(Guid userId, string originalFileName, string contentType, long fileSize, Stream fileStream)
    {
        // Verify user exists
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return new FileUploadResultDto
            {
                Success = false,
                Message = "User not found."
            };
        }

        // Save file to storage
        var (success, storagePath, message) = await _fileStorageService.SaveFileAsync(fileStream, originalFileName);
        if (!success)
        {
            return new FileUploadResultDto
            {
                Success = false,
                Message = message
            };
        }

        // Create file record in database
        var file = new UserFile
        {
            Id = Guid.NewGuid(),
            FileName = Path.GetFileName(originalFileName),
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        await _dbContext.Files.AddAsync(file);
        await _dbContext.SaveChangesAsync();

        return new FileUploadResultDto
        {
            Success = true,
            Message = "File uploaded successfully.",
            File = new FileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.FileSize,
                UploadedAt = file.UploadedAt,
                HasActiveShare = false
            }
        };
    }

    /// <summary>
    /// Retrieves all files owned by a specific user
    /// </summary>
    /// <param name="userId">ID of the user whose files to retrieve</param>
    /// <returns>Collection of file DTOs</returns>
    public async Task<IEnumerable<FileDto>> GetUserFilesAsync(Guid userId)
    {
        var files = await _dbContext.Files
            .Include(f => f.Share)
            .Where(f => f.OwnerId == userId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();

        return files.Select(f => new FileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            ContentType = f.ContentType,
            FileSize = f.FileSize,
            UploadedAt = f.UploadedAt,
            HasActiveShare = f.Share != null && f.Share.IsActive
        });
    }

    /// <summary>
    /// Deletes a file and its associated storage
    /// </summary>
    /// <param name="userId">ID of the user requesting deletion</param>
    /// <param name="fileId">ID of the file to delete</param>
    /// <returns>Success status and message</returns>
    public async Task<(bool Success, string Message)> DeleteFileAsync(Guid userId, Guid fileId)
    {
        // Find the file
        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null)
        {
            return (false, "File not found");
        }

        // Check if user is admin
        var isAdmin = await _dbContext.Users
            .Where(u => u.Id == userId)
            .AnyAsync();

        // Verify ownership or admin status
        if (file.OwnerId != userId && !isAdmin)
        {
            return (false, "You do not have permission to delete this file");
        }

        // Delete physical file
        await _fileStorageService.DeleteFileAsync(file.StoragePath);

        // Remove database record
        _dbContext.Files.Remove(file);
        await _dbContext.SaveChangesAsync();

        return (true, "File deleted successfully");
    }

    /// <summary>
    /// Renames a file
    /// </summary>
    /// <param name="userId">ID of the user requesting rename</param>
    /// <param name="renameDto">DTO containing file ID and new name</param>
    /// <returns>Success status, message, and updated file details</returns>
    public async Task<(bool Success, string Message, FileDto? File)> RenameFileAsync(Guid userId, FileRenameDto renameDto)
    {
        // Find the file
        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == renameDto.FileId);

        if (file == null)
        {
            return (false, "File not found", null);
        }

        // Verify ownership
        if (file.OwnerId != userId)
        {
            return (false, "You do not have permission to rename this file", null);
        }

        // Update filename
        file.FileName = renameDto.NewFileName;
        await _dbContext.SaveChangesAsync();

        return (true, "File renamed successfully", new FileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            UploadedAt = file.UploadedAt,
            HasActiveShare = file.Share != null && file.Share.IsActive
        });
    }

    /// <summary>
    /// Creates a shareable link for a file
    /// </summary>
    /// <param name="userId">ID of the user creating the share</param>
    /// <param name="shareDto">DTO containing share details</param>
    /// <returns>Share result with code and expiration details</returns>
    public async Task<FileShareResultDto> CreateFileShareAsync(Guid userId, FileShareDto shareDto)
    {   
        // Find the file
        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == shareDto.FileId);

        if (file == null)
        {
            Console.WriteLine($"File not found: {shareDto.FileId}");
            return null;
        }

        // Verify file ownership
        if (file.OwnerId != userId)
        {
            Console.WriteLine($"User {userId} is not the owner of file {shareDto.FileId}");
            return null;
        }

        // Generate share code
        string shareCode = GenerateShareCode();
        Console.WriteLine($"Generated share code: {shareCode}");

        // Set expiration time
        var expiresAt = DateTime.UtcNow.AddDays(shareDto.ExpirationDays);
        
        // Find existing share record
        var existingShare = await _dbContext.ShareFiles
            .FirstOrDefaultAsync(s => s.FileId == shareDto.FileId);
            
        // Completely remove existing share record (if exists)
        if (existingShare != null)
        {
            Console.WriteLine($"Removing existing share record: {existingShare.Id}");
            _dbContext.ShareFiles.Remove(existingShare);
            await _dbContext.SaveChangesAsync();
        }
        
        // Create new share record
        var newShare = new Entities.ShareFile
        {
            Id = Guid.NewGuid(),
            FileId = file.Id,
            ShareCode = shareCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
        
        Console.WriteLine($"Creating new share record: {newShare.Id}");
        await _dbContext.ShareFiles.AddAsync(newShare);
        
        try
        {
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Share record saved successfully");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving share record: {ex.Message}");
            throw;
        }

        return new FileShareResultDto
        {
            ShareCode = shareCode,
            ExpiresAt = expiresAt,
            FileName = file.FileName
        };
    }

    /// <summary>
    /// Downloads a file by its ID
    /// </summary>
    /// <param name="userId">ID of the user requesting download</param>
    /// <param name="fileId">ID of the file to download</param>
    /// <returns>File stream and metadata if successful</returns>
    public async Task<(bool Success, string Message, Stream? FileStream, string? FileName, string? ContentType)> DownloadFileByIdAsync(Guid userId, Guid fileId)
    {
        // Find the file
        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null)
        {
            return (false, "File does not exist", null, null, null);
        }

        // Verify file ownership (unless admin)
        var isAdmin = await _dbContext.Users
            .Where(u => u.Id == userId && u.Role == UserRole.Admin)
            .AnyAsync();

        if (file.OwnerId != userId && !isAdmin)
        {
            return (false, "You do not have permission to download this file", null, null, null);
        }

        // Get file stream
        var (success, fileStream, message) = await _fileStorageService.GetFileAsync(file.StoragePath);
        if (!success)
        {
            return (false, message, null, null, null);
        }

        return (true, "File retrieved successfully", fileStream, file.OriginalFileName, file.ContentType);
    }

    /// <summary>
    /// Downloads a file using a share code
    /// </summary>
    /// <param name="shareCode">Share code for the file</param>
    /// <returns>File stream and metadata if successful</returns>
    public async Task<(bool Success, string Message, Stream? FileStream, string? FileName, string? ContentType)> DownloadFileByShareCodeAsync(string shareCode)
    {
        // Find share record
        var share = await _dbContext.ShareFiles
            .Include(s => s.File)
            .FirstOrDefaultAsync(s => s.ShareCode == shareCode);

        if (share == null)
        {
            return (false, "Share link does not exist", null, null, null);
        }

        // Verify if share is valid
        if (!share.IsActive)
        {
            return (false, "Share link has expired", null, null, null);
        }

        // Get file stream
        var (success, fileStream, message) = await _fileStorageService.GetFileAsync(share.File.StoragePath);
        return !success ? (false, message, null, null, null) : (true, "File retrieved successfully", fileStream, share.File.OriginalFileName, share.File.ContentType);
    }

    /// <summary>
    /// Generates a random 8-character alphanumeric share code
    /// </summary>
    /// <returns>Generated share code</returns>
    private static string GenerateShareCode()
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
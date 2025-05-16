using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConcordCloud.Core.Services
{
    public class FileService : IFileService
    {
        private readonly IAppDbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;

        public FileService(IAppDbContext dbContext, IFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
        }

        public async Task<FileUploadResultDto> UploadFileAsync(Guid userId, string originalFileName, string contentType, long fileSize, Stream fileStream)
        {
            // 验证用户是否存在
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return new FileUploadResultDto
                {
                    Success = false,
                    Message = "用户不存在"
                };
            }

            // 保存文件到存储系统
            var (success, storagePath, message) = await _fileStorageService.SaveFileAsync(fileStream, originalFileName);
            if (!success)
            {
                return new FileUploadResultDto
                {
                    Success = false,
                    Message = message
                };
            }

            // 创建文件记录
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

            // 保存到数据库
            await _dbContext.Files.AddAsync(file);
            await _dbContext.SaveChangesAsync();

            // 返回结果
            return new FileUploadResultDto
            {
                Success = true,
                Message = "文件上传成功",
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

        public async Task<(bool Success, string Message)> DeleteFileAsync(Guid userId, Guid fileId)
        {
            // 查找文件
            var file = await _dbContext.Files
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return (false, "文件不存在");
            }

            // 验证文件所有权（除非是管理员）
            var isAdmin = await _dbContext.Users
                .Where(u => u.Id == userId && u.Role == UserRole.Admin)
                .AnyAsync();

            if (file.OwnerId != userId && !isAdmin)
            {
                return (false, "您没有权限删除此文件");
            }

            // 删除物理文件
            await _fileStorageService.DeleteFileAsync(file.StoragePath);

            // 删除数据库记录
            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();

            return (true, "文件删除成功");
        }

        public async Task<(bool Success, string Message, FileDto File)> RenameFileAsync(Guid userId, FileRenameDto renameDto)
        {
            // 查找文件
            var file = await _dbContext.Files
                .FirstOrDefaultAsync(f => f.Id == renameDto.FileId);

            if (file == null)
            {
                return (false, "文件不存在", null);
            }

            // 验证文件所有权（除非是管理员）
            var isAdmin = await _dbContext.Users
                .Where(u => u.Id == userId && u.Role == UserRole.Admin)
                .AnyAsync();

            if (file.OwnerId != userId && !isAdmin)
            {
                return (false, "您没有权限重命名此文件", null);
            }

            // 更新文件名
            file.FileName = renameDto.NewFileName;
            await _dbContext.SaveChangesAsync();

            return (true, "文件重命名成功", new FileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.FileSize,
                UploadedAt = file.UploadedAt,
                HasActiveShare = file.Share != null && file.Share.IsActive
            });
        }

        public async Task<FileShareResultDto> CreateFileShareAsync(Guid userId, FileShareDto shareDto)
        {
            try
            {
                Console.WriteLine($"开始创建文件分享 - 用户ID: {userId}, 文件ID: {shareDto.FileId}");
                
                // 使用事务来确保数据一致性
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                
                try
                {
                    // 查找文件 - 使用AsNoTracking以避免EF Core跟踪实体状态
                    var file = await _dbContext.Files
                        .AsNoTracking()
                        .FirstOrDefaultAsync(f => f.Id == shareDto.FileId);

                    if (file == null)
                    {
                        Console.WriteLine($"文件不存在: {shareDto.FileId}");
                        return null;
                    }

                    // 验证文件所有权
                    if (file.OwnerId != userId)
                    {
                        Console.WriteLine($"用户 {userId} 不是文件 {shareDto.FileId} 的所有者");
                        return null;
                    }

                    // 生成分享码
                    string shareCode = GenerateShareCode();
                    Console.WriteLine($"生成分享码: {shareCode}");

                    // 设置过期时间
                    var expiresAt = DateTime.UtcNow.AddDays(shareDto.ExpirationDays);
                    
                    // 查询是否已存在分享
                    var existingShare = await _dbContext.FileShares
                        .FirstOrDefaultAsync(s => s.FileId == shareDto.FileId);

                    if (existingShare != null)
                    {
                        // 如果存在分享，直接删除旧的分享记录
                        Console.WriteLine($"删除现有分享记录: {existingShare.Id}");
                        _dbContext.FileShares.Remove(existingShare);
                        await _dbContext.SaveChangesAsync();
                    }
                    
                    // 创建全新的分享记录
                    var newShare = new Entities.FileShare
                    {
                        Id = Guid.NewGuid(),
                        FileId = shareDto.FileId,
                        ShareCode = shareCode,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = expiresAt
                    };
                    
                    // 添加新的分享记录
                    Console.WriteLine($"创建新分享记录: {newShare.Id}");
                    await _dbContext.FileShares.AddAsync(newShare);
                    await _dbContext.SaveChangesAsync();
                    
                    // 提交事务
                    await transaction.CommitAsync();
                    Console.WriteLine("分享记录保存成功");

                    return new FileShareResultDto
                    {
                        ShareCode = shareCode,
                        ExpiresAt = expiresAt,
                        FileName = file.FileName
                    };
                }
                catch (Exception ex)
                {
                    // 如果出现任何错误，回滚事务
                    Console.WriteLine($"保存分享记录时出错: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                // 重新抛出并发异常，让控制器处理
                throw;
            }
            catch (Exception ex)
            {
                // 记录其他异常
                Console.WriteLine($"创建文件分享出错: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Success, string Message, Stream FileStream, string FileName, string ContentType)> DownloadFileByIdAsync(Guid userId, Guid fileId)
        {
            // 查找文件
            var file = await _dbContext.Files
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return (false, "文件不存在", null, null, null);
            }

            // 验证文件所有权（除非是管理员）
            var isAdmin = await _dbContext.Users
                .Where(u => u.Id == userId && u.Role == UserRole.Admin)
                .AnyAsync();

            if (file.OwnerId != userId && !isAdmin)
            {
                return (false, "您没有权限下载此文件", null, null, null);
            }

            // 获取文件流
            var (success, fileStream, message) = await _fileStorageService.GetFileAsync(file.StoragePath);
            if (!success)
            {
                return (false, message, null, null, null);
            }

            return (true, "文件获取成功", fileStream, file.OriginalFileName, file.ContentType);
        }

        public async Task<(bool Success, string Message, Stream FileStream, string FileName, string ContentType)> DownloadFileByShareCodeAsync(string shareCode)
        {
            // 查找分享记录
            var share = await _dbContext.FileShares
                .Include(s => s.File)
                .FirstOrDefaultAsync(s => s.ShareCode == shareCode);

            if (share == null)
            {
                return (false, "分享链接不存在", null, null, null);
            }

            // 验证分享是否有效
            if (!share.IsActive)
            {
                return (false, "分享链接已过期", null, null, null);
            }

            // 获取文件流
            var (success, fileStream, message) = await _fileStorageService.GetFileAsync(share.File.StoragePath);
            if (!success)
            {
                return (false, message, null, null, null);
            }

            return (true, "文件获取成功", fileStream, share.File.OriginalFileName, share.File.ContentType);
        }

        private string GenerateShareCode()
        {
            // 生成8位随机字符作为分享码
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
} 
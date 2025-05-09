using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConcordCloud.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAppDbContext _dbContext;

        public AdminService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool Success, string Message, AdminDto? Admin)> LoginAsync(AdminLoginDto loginDto)
        {
            var admin = await _dbContext.Admins
                .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

            if (admin == null)
            {
                return (false, "管理员账号不存在", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
            {
                return (false, "密码错误", null);
            }

            // 更新最后登录时间
            admin.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var adminDto = new AdminDto
            {
                Id = admin.Id,
                Email = admin.Email,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };

            return (true, "登录成功", adminDto);
        }

        public async Task<AdminDto?> GetAdminByIdAsync(Guid adminId)
        {
            var admin = await _dbContext.Admins
                .FirstOrDefaultAsync(a => a.Id == adminId);

            if (admin == null)
            {
                return null;
            }

            return new AdminDto
            {
                Id = admin.Id,
                Email = admin.Email,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid adminId, AdminUpdatePasswordDto passwordDto)
        {
            var admin = await _dbContext.Admins.FindAsync(adminId);
            if (admin == null)
            {
                return (false, "管理员账号不存在");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, admin.PasswordHash))
            {
                return (false, "当前密码错误");
            }

            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return (false, "新密码与确认密码不匹配");
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "密码修改成功");
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            var users = await _dbContext.Users
                .Include(u => u.Files)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    FilesCount = u.Files.Count,
                    TotalStorageUsed = u.Files.Sum(f => f.FileSize)
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserListDto?> GetUserDetailsAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Files)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserListDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                FilesCount = user.Files.Count,
                TotalStorageUsed = user.Files.Sum(f => f.FileSize)
            };
        }

        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(Guid userId, AdminResetUserPasswordDto resetDto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "用户不存在");
            }

            if (resetDto.NewPassword != resetDto.ConfirmPassword)
            {
                return (false, "新密码与确认密码不匹配");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "用户密码重置成功");
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "用户不存在");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return (true, "用户删除成功");
        }

        public async Task<List<FileDto>> GetAllFilesAsync()
        {
            return await _dbContext.Files
                .Include(f => f.Owner)
                .Include(f => f.Share)
                .Select(f => new FileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    UploadedAt = f.UploadedAt,
                    HasActiveShare = f.Share != null && f.Share.IsActive
                })
                .ToListAsync();
        }

        public async Task<List<FileDto>> GetUserFilesAsync(Guid userId)
        {
            return await _dbContext.Files
                .Include(f => f.Share)
                .Where(f => f.OwnerId == userId)
                .Select(f => new FileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    UploadedAt = f.UploadedAt,
                    HasActiveShare = f.Share != null && f.Share.IsActive
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> DeleteFileAsync(Guid fileId)
        {
            var file = await _dbContext.Files.FindAsync(fileId);
            if (file == null)
            {
                return (false, "文件不存在");
            }

            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();

            return (true, "文件删除成功");
        }

        public async Task<PlatformStatisticsDto> GetPlatformStatisticsAsync()
        {
            var totalUsers = await _dbContext.Users.CountAsync();
            var totalFiles = await _dbContext.Files.CountAsync();
            var totalStorage = await _dbContext.Files.SumAsync(f => (long)f.FileSize);

            var lastWeekDate = DateTime.UtcNow.AddDays(-7);
            var newUsersLastWeek = await _dbContext.Users
                .Where(u => u.CreatedAt >= lastWeekDate)
                .CountAsync();

            var newFilesLastWeek = await _dbContext.Files
                .Where(f => f.UploadedAt >= lastWeekDate)
                .CountAsync();

            var topUsers = await _dbContext.Users
                .Include(u => u.Files)
                .OrderByDescending(u => u.Files.Sum(f => f.FileSize))
                .Take(10)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    FilesCount = u.Files.Count,
                    TotalStorageUsed = u.Files.Sum(f => f.FileSize)
                })
                .ToListAsync();

            return new PlatformStatisticsDto
            {
                TotalUsers = totalUsers,
                TotalFiles = totalFiles,
                TotalStorageUsed = totalStorage,
                NewUsersLastWeek = newUsersLastWeek,
                NewFilesLastWeek = newFilesLastWeek,
                TopUsers = topUsers
            };
        }

        public async Task<(bool Success, string Message)> InitializeDefaultAdminAsync(AdminCreateDto adminDto)
        {
            // 检查是否已存在管理员
            var adminExists = await _dbContext.Admins.AnyAsync();
            if (adminExists)
            {
                return (false, "管理员账号已存在，无法创建默认管理员");
            }

            if (adminDto.Password != adminDto.ConfirmPassword)
            {
                return (false, "密码与确认密码不匹配");
            }

            // 创建默认管理员账号
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                Email = adminDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDto.Password),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.Admin
            };

            _dbContext.Admins.Add(admin);
            await _dbContext.SaveChangesAsync();

            return (true, "默认管理员账号创建成功");
        }
    }
} 
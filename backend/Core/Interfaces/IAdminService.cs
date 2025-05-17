using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;

namespace ConcordCloud.Core.Interfaces
{
    public interface IAdminService
    {
        // 管理员身份认证
        Task<(bool Success, string Message, AdminDto? Admin)> LoginAsync(AdminLoginDto loginDto);
        Task<AdminDto?> GetAdminByIdAsync(Guid adminId);
        Task<(bool Success, string Message)> ChangePasswordAsync(Guid adminId, AdminUpdatePasswordDto passwordDto);
        
        // 用户管理
        Task<List<UserListDto>> GetAllUsersAsync();
        Task<UserListDto?> GetUserDetailsAsync(Guid userId);
        Task<(bool Success, string Message)> ResetUserPasswordAsync(Guid userId, AdminResetUserPasswordDto resetDto);
        Task<(bool Success, string Message)> DeleteUserAsync(Guid userId);
        
        // 文件管理
        Task<List<FileDto>> GetAllFilesAsync();
        Task<List<FileDto>> GetUserFilesAsync(Guid userId);
        Task<(bool Success, string Message)> DeleteFileAsync(Guid fileId);
        
        // 统计数据
        Task<PlatformStatisticsDto> GetPlatformStatisticsAsync();
        
        // 初始化管理员账号（如果不存在）
        Task<(bool Success, string Message)> InitializeDefaultAdminAsync(AdminCreateDto adminDto);
    }
} 
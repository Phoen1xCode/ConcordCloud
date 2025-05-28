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
    /// <summary>
    /// Service responsible for handling administrative operations including user management,
    /// platform statistics, and system configuration
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly IAppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the AdminService
        /// </summary>
        /// <param name="dbContext">Database context for administrative operations</param>
        public AdminService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Authenticates an administrator and logs them in
        /// </summary>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>Result of the login operation including admin details if successful</returns>
        public async Task<(bool Success, string Message, AdminDto? Admin)> LoginAsync(AdminLoginDto loginDto)
        {
            var admin = await _dbContext.Admins
                .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

            if (admin == null)
            {
                return (false, "Administrator account does not exist", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
            {
                return (false, "Invalid password", null);
            }

            // Update last login time
            admin.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return (true, "Login successful", new AdminDto
            {
                Id = admin.Id,
                Email = admin.Email,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            });
        }

        /// <summary>
        /// Retrieves administrator details by their ID
        /// </summary>
        /// <param name="adminId">ID of the administrator to retrieve</param>
        /// <returns>Administrator details if found, null otherwise</returns>
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

        /// <summary>
        /// Changes the password for an administrator account
        /// </summary>
        /// <param name="adminId">ID of the administrator changing their password</param>
        /// <param name="passwordDto">DTO containing current and new password details</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid adminId, AdminUpdatePasswordDto passwordDto)
        {
            var admin = await _dbContext.Admins.FindAsync(adminId);
            if (admin == null)
            {
                return (false, "Administrator account does not exist");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, admin.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return (false, "New password and confirmation password do not match");
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "Password changed successfully");
        }

        /// <summary>
        /// Retrieves a list of all users in the system
        /// </summary>
        /// <returns>Collection of user DTOs with their file statistics</returns>
        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            return await _dbContext.Users
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
        }

        /// <summary>
        /// Retrieves detailed information about a specific user
        /// </summary>
        /// <param name="userId">ID of the user to retrieve details for</param>
        /// <returns>User details if found, null otherwise</returns>
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

        /// <summary>
        /// Resets a user's password to a new value
        /// </summary>
        /// <param name="userId">ID of the user whose password to reset</param>
        /// <param name="resetDto">DTO containing new password details</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(Guid userId, AdminResetUserPasswordDto resetDto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User does not exist");
            }

            if (resetDto.NewPassword != resetDto.ConfirmPassword)
            {
                return (false, "New password and confirmation password do not match");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "User password reset successfully");
        }

        /// <summary>
        /// Deletes a user and all associated data from the system
        /// </summary>
        /// <param name="userId">ID of the user to delete</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> DeleteUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User does not exist");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return (true, "User deleted successfully");
        }

        /// <summary>
        /// Retrieves a list of all files in the system
        /// </summary>
        /// <returns>Collection of file DTOs</returns>
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

        /// <summary>
        /// Retrieves all files owned by a specific user
        /// </summary>
        /// <param name="userId">ID of the user whose files to retrieve</param>
        /// <returns>Collection of file DTOs</returns>
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

        /// <summary>
        /// Deletes a file from the system
        /// </summary>
        /// <param name="fileId">ID of the file to delete</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> DeleteFileAsync(Guid fileId)
        {
            var file = await _dbContext.Files.FindAsync(fileId);
            if (file == null)
            {
                return (false, "File does not exist");
            }

            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();

            return (true, "File deleted successfully");
        }

        /// <summary>
        /// Retrieves platform-wide statistics and metrics
        /// </summary>
        /// <returns>Platform statistics including user counts, file counts, and storage usage</returns>
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

        /// <summary>
        /// Creates the initial administrator account for the system
        /// </summary>
        /// <param name="adminDto">DTO containing administrator creation details</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> InitializeDefaultAdminAsync(AdminCreateDto adminDto)
        {
            // Check if administrator already exists
            var adminExists = await _dbContext.Admins.AnyAsync();
            if (adminExists)
            {
                return (false, "Administrator account already exists, cannot create default administrator");
            }

            if (adminDto.Password != adminDto.ConfirmPassword)
            {
                return (false, "Password and confirmation password do not match");
            }

            // Create default administrator account
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

            return (true, "Default administrator account created successfully");
        }
    }
} 
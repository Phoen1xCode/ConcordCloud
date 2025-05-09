using System;
using System.Collections.Generic;

namespace ConcordCloud.Core.DTOs;

public class AdminLoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class AdminDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminCreateDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class AdminUpdatePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class AdminResetUserPasswordDto
{
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class UserListDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FilesCount { get; set; }
    public long TotalStorageUsed { get; set; }
}

public class PlatformStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalFiles { get; set; }
    public long TotalStorageUsed { get; set; }
    public int NewUsersLastWeek { get; set; }
    public int NewFilesLastWeek { get; set; }
    public List<UserListDto> TopUsers { get; set; } = new List<UserListDto>();
} 
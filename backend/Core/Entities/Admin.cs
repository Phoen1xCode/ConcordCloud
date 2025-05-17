using System;

namespace ConcordCloud.Core.Entities;

public class Admin
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public UserRole Role { get; set; } = UserRole.Admin;
}

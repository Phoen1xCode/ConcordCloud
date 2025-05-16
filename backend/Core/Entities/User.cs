using System;
using System.Collections.Generic;

namespace ConcordCloud.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public virtual ICollection<UserFile> Files { get; set; } = new List<UserFile>();
    }

    public enum UserRole
    {
        User,
        Admin
    }
} 
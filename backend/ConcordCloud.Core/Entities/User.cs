using System;
using System.Collections.Generic;

namespace ConcordCloud.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = "User"; // 默认角色为普通用户
        public virtual ICollection<File> Files { get; set; }
    }
} 
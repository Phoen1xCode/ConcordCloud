using System;
using System.Threading;
using System.Threading.Tasks;
using ConcordCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcordCloud.Core.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<UserFile> Files { get; set; }
        DbSet<Entities.FileShare> FileShares { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 
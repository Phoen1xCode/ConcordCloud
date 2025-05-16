using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConcordCloud.Infrastructure.Database
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> Files { get; set; }
        public DbSet<Core.Entities.FileShare> FileShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 配置
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // UserFile 配置
            modelBuilder.Entity<UserFile>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // FileShare 配置
            modelBuilder.Entity<Core.Entities.FileShare>()
                .HasOne(s => s.File)
                .WithOne(f => f.Share)
                .HasForeignKey<Core.Entities.FileShare>(s => s.FileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Core.Entities.FileShare>()
                .HasIndex(s => s.ShareCode)
                .IsUnique();
        }
    }
} 
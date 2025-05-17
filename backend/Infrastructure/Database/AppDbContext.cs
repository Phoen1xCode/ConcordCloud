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
        public DbSet<ShareFile> ShareFiles  { get; set; }
        public DbSet<Admin> Admins { get; set; }

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

            // ShareFile 配置
            modelBuilder.Entity<ShareFile>()
                .HasOne(s => s.File)
                .WithOne(f => f.Share)
                .HasForeignKey<ShareFile>(s => s.FileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<ShareFile>()
                .HasIndex(s => s.ShareCode)
                .IsUnique();
                
            // Admin 配置
            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
    }
} 
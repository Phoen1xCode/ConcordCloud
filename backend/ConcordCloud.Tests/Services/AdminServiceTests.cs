using System;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConcordCloud.Tests.Services
{
    public class AdminServiceTests
    {
        private class TestDbContext : DbContext, IAppDbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public DbSet<User> Users { get; set; }
            public DbSet<UserFile> Files { get; set; }
            public DbSet<ShareFile> ShareFiles { get; set; }
            public DbSet<Admin> Admins { get; set; }
            public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => base.SaveChangesAsync(cancellationToken);
        }

        private static AdminService CreateAdminService(DbContextOptions<TestDbContext> options)
        {
            var dbContext = new TestDbContext(options);
            return new AdminService(dbContext);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adminService = CreateAdminService(options);

            // 先创建一个管理员
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.Admin
            };
            using (var context = new TestDbContext(options))
            {
                context.Admins.Add(admin);
                await context.SaveChangesAsync();
            }

            var loginDto = new AdminLoginDto
            {
                Email = "admin@example.com",
                Password = "password123"
            };

            var result = await adminService.LoginAsync(loginDto);

            Assert.True(result.Success);
            Assert.Equal("登录成功", result.Message);
            Assert.NotNull(result.Admin);
            Assert.Equal(admin.Email, result.Admin.Email);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldFail()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adminService = CreateAdminService(options);

            // 先创建一个管理员
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.Admin
            };
            using (var context = new TestDbContext(options))
            {
                context.Admins.Add(admin);
                await context.SaveChangesAsync();
            }

            var loginDto = new AdminLoginDto
            {
                Email = "admin@example.com",
                Password = "wrongpassword"
            };

            var result = await adminService.LoginAsync(loginDto);

            Assert.False(result.Success);
            Assert.Equal("密码错误", result.Message);
            Assert.Null(result.Admin);
        }

        [Fact]
        public async Task GetAdminByIdAsync_WithValidId_ShouldReturnAdmin()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adminService = CreateAdminService(options);

            // 先创建一个管理员
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.Admin
            };
            using (var context = new TestDbContext(options))
            {
                context.Admins.Add(admin);
                await context.SaveChangesAsync();
            }

            var result = await adminService.GetAdminByIdAsync(admin.Id);

            Assert.NotNull(result);
            Assert.Equal(admin.Email, result.Email);
        }

        [Fact]
        public async Task GetAdminByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adminService = CreateAdminService(options);

            var result = await adminService.GetAdminByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
} 
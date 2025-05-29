using System;
using System.Threading;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;
using BCrypt.Net;

namespace ConcordCloud.Tests.Services
{
    public class UserServiceTests
    {
        private class TestDbContext : DbContext, IAppDbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public DbSet<User> Users { get; set; }
            public DbSet<UserFile> Files { get; set; }
            public DbSet<ShareFile> ShareFiles { get; set; }
            public DbSet<Admin> Admins { get; set; }
            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => base.SaveChangesAsync(cancellationToken);
        }

        private static UserService CreateUserService(DbContextOptions<TestDbContext> options)
        {
            var dbContext = new TestDbContext(options);
            return new UserService(dbContext);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldSucceed()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            var registerDto = new UserRegisterDto
            {
                Username = "TestUser",
                Email = "user@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var result = await userService.RegisterAsync(registerDto);

            Assert.True(result.Success);
            Assert.Equal("注册成功", result.Message);
            Assert.NotNull(result.User);
            Assert.Equal(registerDto.Email, result.User.Email);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldFail()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            // 先注册一个用户
            await userService.RegisterAsync(new UserRegisterDto
            {
                Username = "user1",
                Email = "existing@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            });

            // 再注册同邮箱
            var registerDto = new UserRegisterDto
            {
                Username = "user2",
                Email = "existing@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var result = await userService.RegisterAsync(registerDto);

            Assert.False(result.Success);
            Assert.Equal("该邮箱已被使用。", result.Message);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Username = "TestUser",
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };
            using (var context = new TestDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var loginDto = new UserLoginDto
            {
                Email = "user@example.com",
                Password = "password123"
            };

            var result = await userService.LoginAsync(loginDto);

            Assert.True(result.Success);
            Assert.Equal("登录成功", result.Message);
            Assert.NotNull(result.User);
            Assert.Equal(user.Email, result.User.Email);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldFail()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            var registerDto = new UserRegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };
            await userService.RegisterAsync(registerDto);

            var loginDto = new UserLoginDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var result = await userService.LoginAsync(loginDto);

            Assert.False(result.Success);
            Assert.Equal("密码错误。", result.Message);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Username = "TestUser",
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };
            using (var context = new TestDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var result = await userService.GetUserByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userService = CreateUserService(options);

            var result = await userService.GetUserByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
    }
} 
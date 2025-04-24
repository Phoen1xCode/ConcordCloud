using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using ConcordCloud.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ConcordCloud.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IAppDbContext> _mockDbContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockDbContext = new Mock<IAppDbContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // 配置JWT密钥
            _mockConfiguration.Setup(x => x["Jwt:Key"])
                .Returns("TestSecretKey12345678901234567890");

            _userService = new UserService(_mockDbContext.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldSucceed()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var users = new List<User>();
            var mockSet = new Mock<DbSet<User>>();
            
            _mockDbContext.Setup(x => x.Users)
                .Returns(mockSet.Object);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(registerDto.Email, result.User.Email);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldFail()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var existingUser = new User { Email = "existing@example.com" };
            var mockSet = new Mock<DbSet<User>>();
            mockSet.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                .ReturnsAsync(true);

            _mockDbContext.Setup(x => x.Users)
                .Returns(mockSet.Object);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("已被注册", result.Message);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                Role = UserRole.User
            };

            var mockSet = new Mock<DbSet<User>>();
            mockSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
                .ReturnsAsync(user);

            _mockDbContext.Setup(x => x.Users)
                .Returns(mockSet.Object);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.NotNull(result.Token);
            Assert.Equal(user.Email, result.User.Email);
        }
    }
} 
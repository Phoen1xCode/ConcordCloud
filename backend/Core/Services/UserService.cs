using System;
using System.Linq;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;
using ConcordCloud.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConcordCloud.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IAppDbContext _dbContext;

        public UserService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<(bool Success, string Message, UserDto User)> RegisterAsync(UserRegisterDto registerDto)
        {
            // 检查邮箱是否已被使用
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, "该邮箱已被注册", null);
            }

            // 验证密码是否一致
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return (false, "两次输入的密码不一致", null);
            }

            // 创建用户实体
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            // 保存到数据库
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // 返回结果
            return (true, "注册成功", new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<(bool Success, string Message, UserDto User)> LoginAsync(UserLoginDto loginDto)
        {
            // 查找用户
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return (false, "用户不存在", null);
            }

            // 验证密码
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, "密码错误", null);
            }

            // 更新最后登录时间
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // 返回结果
            return (true, "登录成功", new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> IsAdmin(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            return user != null && user.Role == UserRole.Admin;
        }

    }
} 
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
        
        // register user
        public async Task<(bool Success, string Message, UserDto? User)> RegisterAsync(UserRegisterDto registerDto)
        {
            // check if the email has been used
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, "该邮箱已被使用。", null);
            }

            // check if the username has been used
            if (await _dbContext.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return (false, "该用户名已被使用。", null);
            }

            // check if the password is consistent
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return (false, "密码不一致。", null);
            }

            // create user entity 
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            // save to database
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // return the result
            return (true, "注册成功", new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        // login user   
        public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(UserLoginDto loginDto)
        {
            // find user by email or username
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => 
                u.Email == loginDto.Email || u.Username == loginDto.Email);
            
            if (user == null)
            {
                return (false, "用户不存在。", null);
            }

            // check if the password is correct
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, "密码错误。", null);
            }

            // update the last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // return the result
            return (true, "登录成功", new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        // get user by id
        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, UserUpdatePasswordDto passwordDto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "用户不存在");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, user.PasswordHash))
            {
                return (false, "当前密码错误");
            }

            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return (false, "新密码与确认密码不匹配");
            }

            if (passwordDto.NewPassword.Length < 6)
            {
                return (false, "新密码长度至少为6位");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "密码修改成功");
        }
    }
} 
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
                return (false, "The email has been used.", null);
            }

            // check if the password is consistent
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return (false, "The password is not consistent.", null);
            }

            // create user entity 
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            // save to database
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // return the result
            return (true, "Register successfully", new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        // login user   
        public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(UserLoginDto loginDto)
        {
            // find user
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            
            if (user == null)
            {
                return (false, "User does not exist.", null);
            }

            // check if the password is correct
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, "The password is incorrect.", null);
            }

            // update the last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // return the result
            return (true, "Login successfully", new UserDto
            {
                Id = user.Id,
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
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
} 
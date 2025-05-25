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
    /// <summary>
    /// Service responsible for handling user operations including registration, authentication,
    /// and profile management
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IAppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the UserService
        /// </summary>
        /// <param name="dbContext">Database context for user operations</param>
        public UserService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="registerDto">DTO containing user registration details</param>
        /// <returns>Result of the registration operation including user details if successful</returns>
        public async Task<(bool Success, string Message, UserDto? User)> RegisterAsync(UserRegisterDto registerDto)
        {
            // Check if the email has been used
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, "This email is already in use.", null);
            }

            // Check if the username has been used
            if (await _dbContext.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return (false, "This username is already in use.", null);
            }

            // Check if the password is consistent
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return (false, "Passwords do not match.", null);
            }

            // Create user entity 
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            // Save to database
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Return the result
            return (true, "Registration successful", new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Authenticates a user and logs them in
        /// </summary>
        /// <param name="loginDto">DTO containing login credentials</param>
        /// <returns>Result of the login operation including user details if successful</returns>
        public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(UserLoginDto loginDto)
        {
            // Find user by email or username
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => 
                u.Email == loginDto.Email || u.Username == loginDto.Email);
            
            if (user == null)
            {
                return (false, "User does not exist.", null);
            }

            // Check if the password is correct
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, "Invalid password.", null);
            }

            // Update the last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Return the result
            return (true, "Login successful", new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Retrieves user details by their ID
        /// </summary>
        /// <param name="userId">ID of the user to retrieve</param>
        /// <returns>User details if found, null otherwise</returns>
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

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">ID of the user changing their password</param>
        /// <param name="passwordDto">DTO containing current and new password details</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, UserUpdatePasswordDto passwordDto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User does not exist");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return (false, "New password and confirmation password do not match");
            }

            if (passwordDto.NewPassword.Length < 6)
            {
                return (false, "New password must be at least 6 characters long");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            await _dbContext.SaveChangesAsync();

            return (true, "Password changed successfully");
        }

        /// <summary>
        /// Updates a user's username
        /// </summary>
        /// <param name="userId">ID of the user updating their username</param>
        /// <param name="usernameDto">DTO containing new username details</param>
        /// <returns>Success status and message</returns>
        public async Task<(bool Success, string Message)> UpdateUsernameAsync(Guid userId, UserUpdateUsernameDto usernameDto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User does not exist");
            }

            // Check if new username is already in use
            if (await _dbContext.Users.AnyAsync(u => u.Username == usernameDto.NewUsername && u.Id != userId))
            {
                return (false, "This username is already in use");
            }

            // Check username length
            if (string.IsNullOrWhiteSpace(usernameDto.NewUsername) || usernameDto.NewUsername.Length < 3)
            {
                return (false, "Username must be at least 3 characters long");
            }

            // Check for invalid characters in username
            if (usernameDto.NewUsername.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-'))
            {
                return (false, "Username can only contain letters, numbers, underscores, and hyphens");
            }

            user.Username = usernameDto.NewUsername;
            await _dbContext.SaveChangesAsync();

            return (true, "Username updated successfully");
        }
    }
} 
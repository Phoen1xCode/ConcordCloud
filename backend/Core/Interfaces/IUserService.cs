using System;
using System.Threading.Tasks;
using ConcordCloud.Core.DTOs;
using ConcordCloud.Core.Entities;

namespace ConcordCloud.Core.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string Message, UserDto User)> RegisterAsync(UserRegisterDto registerDto);
        Task<(bool Success, string Message, UserDto User)> LoginAsync(UserLoginDto loginDto);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<bool> IsAdmin(Guid userId);
    }
} 
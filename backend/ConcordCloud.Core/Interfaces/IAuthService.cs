using System.Threading.Tasks;
using ConcordCloud.Core.DTOs.Auth;

namespace ConcordCloud.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string token)> LoginAsync(LoginDto loginDto);
        Task<(bool success, string message)> RegisterAsync(RegisterDto registerDto);
        Task<bool> ValidateTokenAsync(string token);
        Task<string> GenerateJwtTokenAsync(string userId, string email, string role);
    }
} 
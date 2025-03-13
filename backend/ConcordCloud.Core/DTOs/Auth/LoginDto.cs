using System.ComponentModel.DataAnnotations;

namespace ConcordCloud.Core.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "邮箱是必填项")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码是必填项")]
        [MinLength(6, ErrorMessage = "密码长度不能小于6位")]
        public string Password { get; set; }
    }
} 
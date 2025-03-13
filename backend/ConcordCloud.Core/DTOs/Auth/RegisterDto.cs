using System.ComponentModel.DataAnnotations;

namespace ConcordCloud.Core.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "邮箱是必填项")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "用户名是必填项")]
        [MinLength(3, ErrorMessage = "用户名长度不能小于3位")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码是必填项")]
        [MinLength(6, ErrorMessage = "密码长度不能小于6位")]
        public string Password { get; set; }

        [Required(ErrorMessage = "确认密码是必填项")]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }
    }
} 
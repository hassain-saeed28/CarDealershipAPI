using System.ComponentModel.DataAnnotations;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.DTOs
{
    public class RegisterRequestDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required, StringLength(6)]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class OtpResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public bool RequiresOtp { get; set; }
    }
}
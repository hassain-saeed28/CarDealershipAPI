using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<OtpResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<AuthResponseDto>> VerifyRegistrationAsync(VerifyOtpDto request);
        Task<ApiResponse<OtpResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<AuthResponseDto>> VerifyLoginAsync(VerifyOtpDto request);
        Task<User?> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
    }
}
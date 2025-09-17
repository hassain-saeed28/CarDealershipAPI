using CarDealershipAPI.Models;

namespace CarDealershipAPI.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, OtpPurpose purpose);
        Task<bool> ValidateOtpAsync(string email, string code, OtpPurpose purpose);
        Task InvalidateOtpAsync(string email, OtpPurpose purpose);
        Task CleanupExpiredOtpAsync();
    }
}
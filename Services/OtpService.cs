using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Data;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.Services
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OtpService> _logger;
        private const int OTP_EXPIRY_MINUTES = 10;

        public OtpService(AppDbContext context, ILogger<OtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(string email, OtpPurpose purpose)
        {
            // Clean up any existing OTP for this email and purpose
            await InvalidateOtpAsync(email, purpose);

            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Create OTP record
            var otpEntity = new OtpCode
            {
                Email = email.ToLower(),
                Code = otpCode,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                IsUsed = false
            };

            _context.OtpCodes.Add(otpEntity);
            await _context.SaveChangesAsync();

            // Simulate OTP delivery (console output as specified)
            Console.WriteLine($"==========================================");
            Console.WriteLine($"OTP DELIVERY SIMULATION");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Purpose: {purpose}");
            Console.WriteLine($"Code: {otpCode}");
            Console.WriteLine($"Expires: {otpEntity.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"==========================================");

            _logger.LogInformation("OTP generated for {Email} with purpose {Purpose}", email, purpose);

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string email, string code, OtpPurpose purpose)
        {
            var otpEntity = await _context.OtpCodes
                .Where(o => o.Email == email.ToLower() 
                         && o.Code == code 
                         && o.Purpose == purpose 
                         && !o.IsUsed 
                         && o.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (otpEntity == null)
            {
                _logger.LogWarning("Invalid OTP attempt for {Email} with purpose {Purpose}", email, purpose);
                return false;
            }

            // Mark as used
            otpEntity.IsUsed = true;
            otpEntity.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP validated successfully for {Email} with purpose {Purpose}", email, purpose);
            return true;
        }

        public async Task InvalidateOtpAsync(string email, OtpPurpose purpose)
        {
            var existingOtps = await _context.OtpCodes
                .Where(o => o.Email == email.ToLower() && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in existingOtps)
            {
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }

            if (existingOtps.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredOtpAsync()
        {
            var expiredOtps = await _context.OtpCodes
                .Where(o => o.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.OtpCodes.RemoveRange(expiredOtps);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired OTP codes", expiredOtps.Count);
        }
    }
}
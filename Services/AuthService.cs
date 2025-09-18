using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IOtpService otpService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _otpService = otpService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<OtpResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    return new ApiResponse<OtpResponseDto>
                    {
                        Success = false,
                        Message = "Email is already registered",
                        Errors = new List<string> { "Email already exists" }
                    };
                }

                // Store user data temporarily in session/cache (for demo, I'll recreate during verification)
                // In production, I'd store this in cache/session

                // Generate OTP
                await _otpService.GenerateOtpAsync(request.Email, OtpPurpose.Registration);

                return new ApiResponse<OtpResponseDto>
                {
                    Success = true,
                    Data = new OtpResponseDto
                    {
                        Message = "OTP sent to your email. Please verify to complete registration.",
                        RequiresOtp = true
                    },
                    Message = "Registration initiated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", request.Email);
                return new ApiResponse<OtpResponseDto>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> VerifyRegistrationAsync(VerifyOtpDto request)
        {
            try
            {
                var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, OtpPurpose.Registration);

                if (!isValidOtp)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or expired OTP",
                        Errors = new List<string> { "OTP verification failed" }
                    };
                }

                // For demo purposes, I'll create a default customer account
                // In production, I'd retrieve the stored registration data
                var user = new User
                {
                    FirstName = "New",
                    LastName = "User",
                    Email = request.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TempPassword123!"), // Temporary password
                    Phone = "",
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        Email = user.Email,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Role = user.Role,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    },
                    Message = "Registration completed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration verification for {Email}", request.Email);
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Registration verification failed",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<ApiResponse<OtpResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var user = await GetUserByEmailAsync(request.Email);

                if (user == null || !user.IsActive)
                {
                    return new ApiResponse<OtpResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Errors = new List<string> { "User not found or inactive" }
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new ApiResponse<OtpResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Errors = new List<string> { "Invalid password" }
                    };
                }

                // Generate OTP for login
                await _otpService.GenerateOtpAsync(request.Email, OtpPurpose.Login);

                return new ApiResponse<OtpResponseDto>
                {
                    Success = true,
                    Data = new OtpResponseDto
                    {
                        Message = "OTP sent to your email. Please verify to complete login.",
                        RequiresOtp = true
                    },
                    Message = "Login initiated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return new ApiResponse<OtpResponseDto>
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> VerifyLoginAsync(VerifyOtpDto request)
        {
            try
            {
                var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, OtpPurpose.Login);

                if (!isValidOtp)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or expired OTP",
                        Errors = new List<string> { "OTP verification failed" }
                    };
                }

                var user = await GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = new List<string> { "User not found" }
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        Email = user.Email,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Role = user.Role,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    },
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login verification for {Email}", request.Email);
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Login verification failed",
                    Errors = new List<string> { "Internal server error" }
                };
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "CarDealershipAPI",
                audience: _configuration["Jwt:Audience"] ?? "CarDealershipAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
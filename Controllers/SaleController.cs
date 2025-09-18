using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using CarDealershipAPI.Services;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SaleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;
        private readonly ILogger<SaleController> _logger;

        public SaleController(AppDbContext context, IOtpService otpService, ILogger<SaleController> logger)
        {
            _context = context;
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Initiate purchase request (requires OTP)
        /// </summary>
        [HttpPost("initiate-purchase")]
        public async Task<IActionResult> InitiatePurchase([FromBody] PurchaseRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Check if vehicle exists and is available
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
                });
            }

            if (vehicle.Status != VehicleStatus.Available)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle is not available for purchase"
                });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User email not found in token"
                });
            }

            await _otpService.GenerateOtpAsync(userEmail, OtpPurpose.PurchaseRequest);

            return Ok(new ApiResponse<OtpResponseDto>
            {
                Success = true,
                Data = new OtpResponseDto
                {
                    Message = "OTP sent to your email. Use it to complete the purchase request.",
                    RequiresOtp = true
                },
                Message = "Purchase initiated successfully"
            });
        }

        /// <summary>
        /// Create purchase request with OTP verification
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> CreatePurchaseRequest([FromBody] PurchaseRequestDto request, [FromQuery] string otpCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User information not found in token"
                });
            }

            // Verify OTP
            var isValidOtp = await _otpService.ValidateOtpAsync(userEmail, otpCode, OtpPurpose.PurchaseRequest);
            if (!isValidOtp)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid or expired OTP"
                });
            }

            // Check if vehicle exists and is available
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
                });
            }

            if (vehicle.Status != VehicleStatus.Available)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle is not available for purchase"
                });
            }

            // Check if user already has a pending request for this vehicle
            var existingRequest = await _context.Sales
                .FirstOrDefaultAsync(s => s.VehicleId == request.VehicleId 
                                    && s.CustomerId == int.Parse(userId) 
                                    && s.Status == SaleStatus.Requested);

            if (existingRequest != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "You already have a pending request for this vehicle"
                });
            }

            var sale = new Sale
            {
                VehicleId = request.VehicleId,
                CustomerId = int.Parse(userId),
                SalePrice = vehicle.Price,
                Status = SaleStatus.Requested,
                RequestedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            _context.Sales.Add(sale);

            // Reserve the vehicle
            vehicle.Status = VehicleStatus.Reserved;
            await _context.SaveChangesAsync();

            // Load navigation properties for response
            await _context.Entry(sale)
                .Reference(s => s.Vehicle)
                .LoadAsync();
            await _context.Entry(sale)
                .Reference(s => s.Customer)
                .LoadAsync();

            var response = new SaleResponseDto
            {
                Id = sale.Id,
                VehicleId = sale.VehicleId,
                CustomerId = sale.CustomerId,
                CustomerName = $"{sale.Customer.FirstName} {sale.Customer.LastName}",
                VehicleMakeModel = $"{sale.Vehicle.Make} {sale.Vehicle.Model}",
                SalePrice = sale.SalePrice,
                Status = sale.Status,
                RequestedAt = sale.RequestedAt,
                CompletedAt = sale.CompletedAt,
                Notes = sale.Notes
            };

            return Ok(new ApiResponse<SaleResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Purchase request created successfully"
            });
        }

        /// <summary>
        /// Get purchase history for current user
        /// </summary>
        [HttpGet("my-purchases")]
        public async Task<IActionResult> GetMyPurchases([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User ID not found in token"
                });
            }

            var totalCount = await _context.Sales
                .Where(s => s.CustomerId == int.Parse(userId))
                .CountAsync();

            var sales = await _context.Sales
                .Where(s => s.CustomerId == int.Parse(userId))
                .Include(s => s.Vehicle)
                .Include(s => s.Customer)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SaleResponseDto
                {
                    Id = s.Id,
                    VehicleId = s.VehicleId,
                    CustomerId = s.CustomerId,
                    CustomerName = $"{s.Customer.FirstName} {s.Customer.LastName}",
                    VehicleMakeModel = $"{s.Vehicle.Make} {s.Vehicle.Model}",
                    SalePrice = s.SalePrice,
                    Status = s.Status,
                    RequestedAt = s.RequestedAt,
                    CompletedAt = s.CompletedAt,
                    Notes = s.Notes
                })
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync();

            var response = new PagedResponse<SaleResponseDto>
            {
                Data = sales,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(new ApiResponse<PagedResponse<SaleResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Purchase history retrieved successfully"
            });
        }

        /// <summary>
        /// Get all sales (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSales([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] SaleStatus? status = null)
        {
            var query = _context.Sales
                .Include(s => s.Vehicle)
                .Include(s => s.Customer)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var sales = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SaleResponseDto
                {
                    Id = s.Id,
                    VehicleId = s.VehicleId,
                    CustomerId = s.CustomerId,
                    CustomerName = $"{s.Customer.FirstName} {s.Customer.LastName}",
                    VehicleMakeModel = $"{s.Vehicle.Make} {s.Vehicle.Model}",
                    SalePrice = s.SalePrice,
                    Status = s.Status,
                    RequestedAt = s.RequestedAt,
                    CompletedAt = s.CompletedAt,
                    Notes = s.Notes
                })
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync();

            var response = new PagedResponse<SaleResponseDto>
            {
                Data = sales,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(new ApiResponse<PagedResponse<SaleResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Sales retrieved successfully"
            });
        }

        /// <summary>
        /// Process sale (Admin only)
        /// </summary>
        [HttpPost("process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessSale([FromBody] ProcessSaleDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var sale = await _context.Sales
                .Include(s => s.Vehicle)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == request.SaleId);

            if (sale == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sale not found"
                });
            }

            if (sale.Status != SaleStatus.Requested)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sale cannot be processed in its current status"
                });
            }

            // Update sale status
            sale.Status = request.Status;
            sale.Notes = request.Notes;

            if (request.Status == SaleStatus.Completed)
            {
                sale.CompletedAt = DateTime.UtcNow;
                sale.Vehicle.Status = VehicleStatus.Sold;
            }
            else if (request.Status == SaleStatus.Cancelled)
            {
                sale.Vehicle.Status = VehicleStatus.Available; // Make vehicle available again
            }

            await _context.SaveChangesAsync();

            var response = new SaleResponseDto
            {
                Id = sale.Id,
                VehicleId = sale.VehicleId,
                CustomerId = sale.CustomerId,
                CustomerName = $"{sale.Customer.FirstName} {sale.Customer.LastName}",
                VehicleMakeModel = $"{sale.Vehicle.Make} {sale.Vehicle.Model}",
                SalePrice = sale.SalePrice,
                Status = sale.Status,
                RequestedAt = sale.RequestedAt,
                CompletedAt = sale.CompletedAt,
                Notes = sale.Notes
            };

            return Ok(new ApiResponse<SaleResponseDto>
            {
                Success = true,
                Data = response,
                Message = $"Sale {request.Status.ToString().ToLower()} successfully"
            });
        }
    }
}
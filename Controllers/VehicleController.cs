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
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(AppDbContext context, IOtpService otpService, ILogger<VehicleController> logger)
        {
            _context = context;
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicles with optional filtering (available to all authenticated users)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetVehicles([FromQuery] VehicleFilterDto filter)
        {
            var query = _context.Vehicles.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Make))
                query = query.Where(v => v.Make.ToLower().Contains(filter.Make.ToLower()));

            if (!string.IsNullOrEmpty(filter.Model))
                query = query.Where(v => v.Model.ToLower().Contains(filter.Model.ToLower()));

            if (filter.MinYear.HasValue)
                query = query.Where(v => v.Year >= filter.MinYear.Value);

            if (filter.MaxYear.HasValue)
                query = query.Where(v => v.Year <= filter.MaxYear.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(v => v.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(v => v.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.Color))
                query = query.Where(v => v.Color.ToLower().Contains(filter.Color.ToLower()));

            if (!string.IsNullOrEmpty(filter.FuelType))
                query = query.Where(v => v.FuelType.ToLower().Contains(filter.FuelType.ToLower()));

            if (!string.IsNullOrEmpty(filter.Transmission))
                query = query.Where(v => v.Transmission.ToLower().Contains(filter.Transmission.ToLower()));

            if (filter.Status.HasValue)
                query = query.Where(v => v.Status == filter.Status.Value);

            // Customers can only see available vehicles
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                query = query.Where(v => v.Status == VehicleStatus.Available);
            }

            var totalCount = await query.CountAsync();
            var vehicles = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    VIN = v.VIN,
                    Price = v.Price,
                    Mileage = v.Mileage,
                    FuelType = v.FuelType,
                    Transmission = v.Transmission,
                    Description = v.Description,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            var response = new PagedResponse<VehicleResponseDto>
            {
                Data = vehicles,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return Ok(new ApiResponse<PagedResponse<VehicleResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Vehicles retrieved successfully"
            });
        }

        /// <summary>
        /// Get vehicle by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
                });
            }

            // Customers can only see available vehicles
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && vehicle.Status != VehicleStatus.Available)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not available"
                });
            }

            var response = new VehicleResponseDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                Price = vehicle.Price,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                Description = vehicle.Description,
                Status = vehicle.Status,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };

            return Ok(new ApiResponse<VehicleResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Vehicle retrieved successfully"
            });
        }

        /// <summary>
        /// Add new vehicle (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleCreateDto request)
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

            // Check if VIN already exists
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VIN == request.VIN);

            if (existingVehicle != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle with this VIN already exists"
                });
            }

            var vehicle = new Vehicle
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Color = request.Color,
                VIN = request.VIN,
                Price = request.Price,
                Mileage = request.Mileage,
                FuelType = request.FuelType,
                Transmission = request.Transmission,
                Description = request.Description,
                Status = VehicleStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var response = new VehicleResponseDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                Price = vehicle.Price,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                Description = vehicle.Description,
                Status = vehicle.Status,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, 
                new ApiResponse<VehicleResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Vehicle added successfully"
                });
        }

        /// <summary>
        /// Initiate vehicle update (requires OTP) - Admin only
        /// </summary>
        [HttpPost("{id}/initiate-update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitiateVehicleUpdate(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
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

            await _otpService.GenerateOtpAsync(userEmail, OtpPurpose.UpdateVehicle);

            return Ok(new ApiResponse<OtpResponseDto>
            {
                Success = true,
                Data = new OtpResponseDto
                {
                    Message = "OTP sent to your email. Use it to complete the vehicle update.",
                    RequiresOtp = true
                },
                Message = "Update initiated successfully"
            });
        }

        /// <summary>
        /// Update vehicle with OTP verification - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleUpdateDto request, [FromQuery] string otpCode)
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
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User email not found in token"
                });
            }

            // Verify OTP
            var isValidOtp = await _otpService.ValidateOtpAsync(userEmail, otpCode, OtpPurpose.UpdateVehicle);
            if (!isValidOtp)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid or expired OTP"
                });
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
                });
            }

            // Check if VIN is being changed and if it already exists
            if (vehicle.VIN != request.VIN)
            {
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VIN == request.VIN && v.Id != id);

                if (existingVehicle != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vehicle with this VIN already exists"
                    });
                }
            }

            // Update vehicle properties
            vehicle.Make = request.Make;
            vehicle.Model = request.Model;
            vehicle.Year = request.Year;
            vehicle.Color = request.Color;
            vehicle.VIN = request.VIN;
            vehicle.Price = request.Price;
            vehicle.Mileage = request.Mileage;
            vehicle.FuelType = request.FuelType;
            vehicle.Transmission = request.Transmission;
            vehicle.Description = request.Description;
            vehicle.Status = request.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new VehicleResponseDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                Price = vehicle.Price,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                Description = vehicle.Description,
                Status = vehicle.Status,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };

            return Ok(new ApiResponse<VehicleResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Vehicle updated successfully"
            });
        }

        /// <summary>
        /// Delete vehicle (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Vehicle not found"
                });
            }

            // Check if vehicle has any sales
            var hasSales = await _context.Sales.AnyAsync(s => s.VehicleId == id);
            if (hasSales)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cannot delete vehicle with existing sales records"
                });
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Vehicle deleted successfully"
            });
        }
    }
}
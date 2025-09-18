using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(AppDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all customers (Admin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var totalCount = await _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .CountAsync();

            var customers = await _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .Include(u => u.Sales)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new CustomerResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    PurchaseCount = u.Sales.Count(s => s.Status == SaleStatus.Completed)
                })
                .ToListAsync();

            var response = new PagedResponse<CustomerResponseDto>
            {
                Data = customers,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(new ApiResponse<PagedResponse<CustomerResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Customers retrieved successfully"
            });
        }

        /// <summary>
        /// Get customer by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Users
                .Where(u => u.Id == id && u.Role == UserRole.Customer)
                .Include(u => u.Sales)
                .Select(u => new CustomerResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    PurchaseCount = u.Sales.Count(s => s.Status == SaleStatus.Completed)
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Customer not found"
                });
            }

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Data = customer,
                Message = "Customer retrieved successfully"
            });
        }
    }
}
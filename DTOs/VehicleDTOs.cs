using System.ComponentModel.DataAnnotations;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.DTOs
{
    public class VehicleCreateDto
    {
        [Required, StringLength(100)]
        public string Make { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty;
        
        [Required, Range(1900, 2030)]
        public int Year { get; set; }
        
        [StringLength(100)]
        public string Color { get; set; } = string.Empty;
        
        [Required, StringLength(20)]
        public string VIN { get; set; } = string.Empty;
        
        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required, Range(0, int.MaxValue)]
        public int Mileage { get; set; }
        
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Transmission { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
    }

    public class VehicleUpdateDto : VehicleCreateDto
    {
        public VehicleStatus Status { get; set; }
    }

    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class VehicleFilterDto
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public VehicleStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarDealershipAPI.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string Make { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty;
        
        [Required]
        public int Year { get; set; }
        
        [StringLength(100)]
        public string Color { get; set; } = string.Empty;
        
        [Required, StringLength(20)]
        public string VIN { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public int Mileage { get; set; }
        
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Transmission { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

    public enum VehicleStatus
    {
        Available = 0,
        Sold = 1,
        Reserved = 2,
        Maintenance = 3
    }
}
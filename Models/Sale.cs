using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarDealershipAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        
        [Required]
        public int VehicleId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }
        
        public SaleStatus Status { get; set; } = SaleStatus.Requested;
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public Vehicle Vehicle { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }

    public enum SaleStatus
    {
        Requested = 0,
        Approved = 1,
        Completed = 2,
        Cancelled = 3
    }
}
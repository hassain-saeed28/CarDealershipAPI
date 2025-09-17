using System.ComponentModel.DataAnnotations;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.DTOs
{
    public class PurchaseRequestDto
    {
        [Required]
        public int VehicleId { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    public class SaleResponseDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleMakeModel { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public SaleStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class ProcessSaleDto
    {
        [Required]
        public int SaleId { get; set; }
        
        [Required]
        public SaleStatus Status { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
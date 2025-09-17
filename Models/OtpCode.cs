using System.ComponentModel.DataAnnotations;

namespace CarDealershipAPI.Models
{
    public class OtpCode
    {
        public int Id { get; set; }
        
        [Required, StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required, StringLength(6)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public OtpPurpose Purpose { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
    }

    public enum OtpPurpose
    {
        Registration = 0,
        Login = 1,
        PurchaseRequest = 2,
        UpdateVehicle = 3
    }
}
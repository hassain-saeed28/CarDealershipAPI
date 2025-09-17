using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace CarDealershipAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation property
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

    public enum UserRole
    {
        Customer = 0,
        Admin = 1
    }
}
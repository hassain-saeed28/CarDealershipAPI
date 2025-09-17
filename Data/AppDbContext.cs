// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Models;
using BCrypt.Net;

namespace CarDealershipAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasConversion<int>();
            });

            // Vehicle configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasIndex(v => v.VIN).IsUnique();
                entity.Property(v => v.VIN).IsRequired().HasMaxLength(20);
                entity.Property(v => v.Make).IsRequired().HasMaxLength(100);
                entity.Property(v => v.Model).IsRequired().HasMaxLength(100);
                entity.Property(v => v.Price).HasColumnType("decimal(18,2)");
                entity.Property(v => v.Status).HasConversion<int>();
            });

            // Sale configuration
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Status).HasConversion<int>();

                // Relationships
                entity.HasOne(s => s.Vehicle)
                      .WithMany(v => v.Sales)
                      .HasForeignKey(s => s.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Customer)
                      .WithMany(u => u.Sales)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OtpCode configuration
            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Email).IsRequired().HasMaxLength(150);
                entity.Property(o => o.Code).IsRequired().HasMaxLength(6);
                entity.Property(o => o.Purpose).HasConversion<int>();
                entity.HasIndex(o => new { o.Email, o.Code, o.Purpose });
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            var adminUser = new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@cardealership.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Phone = "1234567890",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Seed customer user for testing
            var customerUser = new User
            {
                Id = 2,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Phone = "0987654321",
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser, customerUser);

            // Seed 15 sample vehicles
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "Toyota", Model = "Camry", Year = 2022, Color = "White", VIN = "1234567890ABCDEF1", Price = 28500.00m, Mileage = 15000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Reliable sedan with excellent fuel economy", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 2, Make = "Honda", Model = "Civic", Year = 2023, Color = "Blue", VIN = "1234567890ABCDEF2", Price = 25000.00m, Mileage = 8000, FuelType = "Gasoline", Transmission = "Manual", Description = "Sporty and efficient compact car", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 3, Make = "Ford", Model = "F-150", Year = 2021, Color = "Black", VIN = "1234567890ABCDEF3", Price = 42000.00m, Mileage = 25000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Powerful pickup truck for work and play", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 4, Make = "BMW", Model = "X5", Year = 2022, Color = "Silver", VIN = "1234567890ABCDEF4", Price = 65000.00m, Mileage = 18000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Luxury SUV with premium features", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 5, Make = "Tesla", Model = "Model 3", Year = 2023, Color = "Red", VIN = "1234567890ABCDEF5", Price = 45000.00m, Mileage = 5000, FuelType = "Electric", Transmission = "Automatic", Description = "Electric vehicle with autopilot features", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 6, Make = "Audi", Model = "A4", Year = 2022, Color = "White", VIN = "1234567890ABCDEF6", Price = 38000.00m, Mileage = 12000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Luxury sedan with advanced technology", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 7, Make = "Mercedes-Benz", Model = "C-Class", Year = 2021, Color = "Black", VIN = "1234567890ABCDEF7", Price = 45000.00m, Mileage = 20000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Premium sedan with elegant design", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 8, Make = "Chevrolet", Model = "Malibu", Year = 2022, Color = "Gray", VIN = "1234567890ABCDEF8", Price = 26000.00m, Mileage = 14000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Mid-size sedan with modern features", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 9, Make = "Nissan", Model = "Altima", Year = 2023, Color = "Blue", VIN = "1234567890ABCDEF9", Price = 27500.00m, Mileage = 6000, FuelType = "Gasoline", Transmission = "CVT", Description = "Comfortable sedan with good fuel economy", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 10, Make = "Hyundai", Model = "Elantra", Year = 2022, Color = "White", VIN = "1234567890ABCDEF0", Price = 22000.00m, Mileage = 16000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Affordable and reliable compact sedan", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 11, Make = "Jeep", Model = "Wrangler", Year = 2021, Color = "Green", VIN = "1234567890ABCDEFG", Price = 38000.00m, Mileage = 22000, FuelType = "Gasoline", Transmission = "Manual", Description = "Off-road capable SUV with removable top", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 12, Make = "Subaru", Model = "Outback", Year = 2022, Color = "Silver", VIN = "1234567890ABCDEFH", Price = 32000.00m, Mileage = 11000, FuelType = "Gasoline", Transmission = "CVT", Description = "All-wheel drive wagon perfect for adventures", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 13, Make = "Mazda", Model = "CX-5", Year = 2023, Color = "Red", VIN = "1234567890ABCDEFI", Price = 29000.00m, Mileage = 7500, FuelType = "Gasoline", Transmission = "Automatic", Description = "Stylish SUV with premium interior", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 14, Make = "Volkswagen", Model = "Jetta", Year = 2022, Color = "Black", VIN = "1234567890ABCDEFJ", Price = 24500.00m, Mileage = 13000, FuelType = "Gasoline", Transmission = "Automatic", Description = "German engineering in a compact package", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow },
                new Vehicle { Id = 15, Make = "Kia", Model = "Sorento", Year = 2023, Color = "Gray", VIN = "1234567890ABCDEFK", Price = 35000.00m, Mileage = 4000, FuelType = "Gasoline", Transmission = "Automatic", Description = "Three-row SUV with advanced safety features", Status = VehicleStatus.Available, CreatedAt = DateTime.UtcNow }
            };

            modelBuilder.Entity<Vehicle>().HasData(vehicles);
        }
    }
}
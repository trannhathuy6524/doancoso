using GotoCarRental.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CarImage> CarImages { get; set; }
        public DbSet<CarModel3D> CarModel3Ds { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Model3DTemplate> Model3DTemplates { get; set; }
        public DbSet<Province> Provinces { get; set; }
        // Thêm vào class ApplicationDbContext
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính cho CarFeature (many-to-many relationship)
            modelBuilder.Entity<CarFeature>()
                .HasKey(cf => new { cf.CarId, cf.FeatureId });

            // Cấu hình mối quan hệ Car với Brand (one-to-many)
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Brand)
                .WithMany(b => b.Cars)
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ Car với Category (one-to-many)
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Cars)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ Car với User (one-to-many)
            modelBuilder.Entity<Car>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cars)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ Car với CarModel3D (one-to-one)
            modelBuilder.Entity<Car>()
                .HasOne(c => c.CarModel3D)
                .WithOne(cm => cm.Car)
                .HasForeignKey<CarModel3D>(cm => cm.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Car với CarImage (one-to-many)
            modelBuilder.Entity<Car>()
                .HasMany(c => c.CarImages)
                .WithOne(ci => ci.Car)
                .HasForeignKey(ci => ci.CarId)
                .OnDelete(DeleteBehavior.Cascade);


            // Cấu hình mối quan hệ Car với Review (one-to-many)
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Reviews)
                .WithOne(r => r.Car)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Car với Rental (one-to-many)
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Rentals)
                .WithOne(r => r.Car)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ Rental với Payment (one-to-many)
            modelBuilder.Entity<Rental>()
                .HasMany(r => r.Payments)
                .WithOne(p => p.Rental)
                .HasForeignKey(p => p.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ User với Rental (one-to-many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Rentals)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ User với Review (one-to-many)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ many-to-many giữa Car và Feature thông qua CarFeature
            modelBuilder.Entity<CarFeature>()
                .HasOne(cf => cf.Car)
                .WithMany(c => c.CarFeatures)
                .HasForeignKey(cf => cf.CarId);

            modelBuilder.Entity<CarFeature>()
                .HasOne(cf => cf.Feature)
                .WithMany(f => f.CarFeatures)
                .HasForeignKey(cf => cf.FeatureId);

            // Cấu hình các mô hình với các ràng buộc khác (nếu cần)
            modelBuilder.Entity<Car>()
                .Property(c => c.PricePerDay)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Rental>()
                .Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // Cấu hình mối quan hệ Car - CarFeature (cascade delete)
            modelBuilder.Entity<Car>()
                .HasMany(c => c.CarFeatures)
                .WithOne(cf => cf.Car)
                .HasForeignKey(cf => cf.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình Review - cascade delete
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Reviews)
                .WithOne(r => r.Car)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            //Mối quan hệ Car - Province
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Province)
                .WithMany(p => p.Cars)
                .HasForeignKey(c => c.ProvinceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.SeedProvinces();

        }
    }
}

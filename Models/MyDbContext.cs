using Microsoft.EntityFrameworkCore;

namespace Laundry.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options)
        {


        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<Clothes> Clothes { get; set; }
        public DbSet<ClothesService> ClothesServices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<BookingClothes> BookingClothes { get; set; }
        public DbSet<Barcode> Barcodes { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClothesService>()
                .HasKey(cs => new { cs.ClothesId, cs.ServicesId });

            modelBuilder.Entity<ClothesService>()
                .HasOne(cs => cs.Clothes)
                .WithMany(c => c.ClothesServices)
                .HasForeignKey(cs => cs.ClothesId);

            modelBuilder.Entity<ClothesService>()
                .HasOne(cs => cs.Services)
                .WithMany(s => s.ClothesServices)
                .HasForeignKey(cs => cs.ServicesId);

            modelBuilder.Entity<Booking>()
        .HasOne(b => b.User)
        .WithMany(u => u.Bookings)
        .HasForeignKey(b => b.UserId);
        }
    }
}



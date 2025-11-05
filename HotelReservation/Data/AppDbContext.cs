
using HotelReservation.Models; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Bu constructor, Program.cs dosyasından veritabanı ayarlarını alabilmemizi sağlar.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // User için db yi identity kütüphanesi sağlıyor
        public DbSet<Room> Rooms { get; set; }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomTranslation> RoomTranslations { get; set; }
        public DbSet<AmenityTranslation> AmenityTranslations { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RoomTranslation>()
                .HasIndex(t => new { t.RoomId, t.LanguageCode })
                .IsUnique();
        }

    }
}
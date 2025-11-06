using HotelReservation.Models; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // Review ile User arasındaki CASCADE kuralı
            modelBuilder.Entity<Review>()
            .HasOne(review => review.User)
            .WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.NoAction);


            // Review ve Room arasındaki  CASCADE kuralı
                modelBuilder.Entity<Review>()
                .HasOne(review => review.Room)
                .WithMany(room => room.Reviews)
                .HasForeignKey(review => review.RoomId)
                .OnDelete(DeleteBehavior.NoAction);
        }
       
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}

        
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomTranslation> RoomTranslations { get; set; }
        public DbSet<AmenityTranslation> AmenityTranslations { get; set; }
        
    }
}
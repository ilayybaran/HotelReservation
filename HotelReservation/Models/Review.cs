using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; } 

        [DataType(DataType.MultilineText)]
        public string? Comment { get; set; } // Yorum metni (opsiyonel)

        public DateTime DatePosted { get; set; }

      
        // Hangi odanın yorumlandığı
        public int RoomId { get; set; }
        public Room Room { get; set; }

        // Kimin yorum yaptığı
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } 

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
    }
}
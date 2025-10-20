
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Oda Tipi gereklidir.")]
        [Display(Name = "Oda Tipi")]
        public string RoomType { get; set; } 

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Gecelik Fiyat gereklidir.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Gecelik Fiyat")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Kapasite gereklidir.")]
        [Display(Name = "Kapasite")]
        public int Capacity { get; set; }

        [Display(Name = "Müsaitlik Durumu")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Oda Fotoğrafı (URL)")]
        public string? ImageUrl { get; set; }

        // Navigation property
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
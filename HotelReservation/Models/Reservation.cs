using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Giriş tarihi seçmelisiniz.")]
        [DataType(DataType.Date)]
        [Display(Name = "Giriş Tarihi")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Çıkış tarihi seçmelisiniz.")]
        [DataType(DataType.Date)]
        [Display(Name = "Çıkış Tarihi")]
        public DateTime CheckOutDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Toplam Fiyat")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Rezervasyon Tarihi")]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        // Navigation Properties (İlişkisel verileri çekmek için)
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Ödeme Durumu")]
        public string Status { get; set; }

        [Display(Name = "Ödeme Tarihi")]
        public DateTime ? PaymentDate { get; set; }
    }
}
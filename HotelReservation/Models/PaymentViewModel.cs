using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public class PaymentViewModel
    {
        [Required]
        public int ReservationId { get; set; }

        // Bunlar formda gizli tutulacak veya sadece gösterilecek
        public decimal Amount { get; set; }
        public string RoomType { get; set; }
        public string Currency { get; set; }

        [Required(ErrorMessage = "Kart üzerindeki isim zorunludur.")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        [Display(Name = "Kart Numarası")]
        [CreditCard] // Basit format kontrolü
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Ay zorunludur.")]
        [Display(Name = "Ay")]
        [Range(1, 12, ErrorMessage = "Geçersiz ay.")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Yıl zorunludur.")]
        [Display(Name = "Yıl")]
        [Range(2025, 2035, ErrorMessage = "Geçersiz yıl.")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVC zorunludur.")]
        [Display(Name = "CVC")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Geçersiz CVC.")]
        public string CVC { get; set; }
    }
}
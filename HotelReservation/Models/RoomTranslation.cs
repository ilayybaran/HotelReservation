using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public class RoomTranslation
    {
        [Key]
        public int Id { get; set; }
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        [Required]
        [StringLength(10)]
        public string LanguageCode { get; set; }

       
        [StringLength(200)]
        public string RoomType { get; set; }

       
        public string Description { get; set; }

    }
}

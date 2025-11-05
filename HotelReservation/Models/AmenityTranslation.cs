using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public class AmenityTranslation
    {
        public int Id { get; set; }

        [Required]
        public string LanguageCode { get; set; } 

        [Required]
        public string Name { get; set; } 

        public int AmenityId { get; set; }

        // Navigation Property
        public Amenity Amenity { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class Amenity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Olanak Adı")]
        [NotMapped]
        public string Name { get; set; } 

        [Display(Name = "Bootstrap İkon Sınıfı")]
        [StringLength(50)]
        public string? IconClass { get; set; } 

        // Navigation property (Bir olanak birden çok odada olabilir)
        public virtual ICollection<Room> Rooms { get; set; }

        public ICollection<AmenityTranslation> Translations { get; set; } = new List<AmenityTranslation>();

        public Amenity()
        {
            Rooms = new HashSet<Room>();   
        }
    }
}
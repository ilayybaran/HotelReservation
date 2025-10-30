using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public class Amenity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Olanak Adı")]
        public string Name { get; set; } 

        [Display(Name = "Bootstrap İkon Sınıfı")]
        [StringLength(50)]
        public string? IconClass { get; set; } 

        // Navigation property (Bir olanak birden çok odada olabilir)
        public virtual ICollection<Room> Rooms { get; set; }

        public Amenity()
        {
            Rooms = new HashSet<Room>();   
        }
    }
}
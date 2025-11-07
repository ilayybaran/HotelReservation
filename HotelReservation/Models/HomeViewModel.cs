
namespace HotelReservation.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Room> FeaturedRooms { get; set; }
        public IEnumerable<Review> FeatredReviews { get; set; }
        public List<Review> FeaturedReviews { get; set; }
    }
}

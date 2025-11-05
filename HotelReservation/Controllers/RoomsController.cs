
using HotelReservation.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Globalization;

namespace HotelReservation.Controllers
{
    public class RoomsController : Controller
    {
        private readonly AppDbContext _context;


        public RoomsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Search(DateTime checkInDate, DateTime checkOutDate, int guestCount)
        {
            var bookedRoomIds = await _context.Reservations
              .Where(res => res.CheckInDate < checkOutDate && res.CheckOutDate > checkInDate)
              .Select(res => res.RoomId)
              .Distinct()
              .ToListAsync();

            var availableRooms = await _context.Rooms
                      .Include(r => r.Translations)
                      .Where(r => r.IsAvailable && r.Capacity >= guestCount)
              .Where(r => !bookedRoomIds.Contains(r.Id))
              .ToListAsync();

            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";
            foreach (var room in availableRooms)
            {
                var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                        ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);
                if (translation != null)
                {
                    room.RoomType = translation.RoomType;
                    room.Description = translation.Description;
                }
            }

            ViewBag.CheckInDate = checkInDate;
            ViewBag.CheckOutDate = checkOutDate;
            ViewBag.GuestCount = guestCount;
            ViewBag.ResultsCount = availableRooms.Count;

            return View("SearchResults", availableRooms);
        }

        // GET: /Rooms
        public async Task<IActionResult> Index()
        {
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";
            var rooms = await _context.Rooms
                .Include(r => r.Translations) // Çevirileri de sorguya dahil et
                .Where(r => r.IsAvailable)
                .ToListAsync();

            foreach (var room in rooms)
            {

                var translation = room.Translations
                                      .FirstOrDefault(t => t.LanguageCode == currentCulture);

                if (translation == null)
                {
                    translation = room.Translations
                                      .FirstOrDefault(t => t.LanguageCode == defaultCulture);
                }

                // Eğer (bir şekilde) hiç çeviri yoksa, boş geçmesin
                if (translation != null)
                {
                    // View'daki @Model.RoomType alanına bu veriyi ata
                    room.RoomType = translation.RoomType;
                    // View'daki @Model.Description alanına bu veriyi ata
                    room.Description = translation.Description;
                }
                else
                {
                    room.RoomType = "Çeviri Bulunamadı";
                    room.Description = "Açıklama mevcut değil.";
                }
            }

            return View(rooms);
        }

        // GET: /Rooms/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
            .Include(r => r.Translations)
            .Include(r => r.Amenities)
            .ThenInclude(a => a.Translations)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();


            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";

            // ODA'yı çevir (Bu kod sizde zaten vardı ve doğruydu)
            var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                    ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);
            if (translation != null)
            {
                room.RoomType = translation.RoomType;
                room.Description = translation.Description;
            }

            foreach (var amenity in room.Amenities)
            {
                var amenityTranslation = amenity.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                                ?? amenity.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);

                if (amenityTranslation != null)
                {
                    amenity.Name = amenityTranslation.Name; // Amenity'nin adını çevrilmiş metinle değiştir
                }
                else
                {
                    amenity.Name = "[Çeviri yok]";
                }
            }

            return View(room);
        }
    }
}

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
            // ... (Tarih ve kapasite sorguları aynı) ...
            var bookedRoomIds = await _context.Reservations
        .Where(res => res.CheckInDate < checkOutDate && res.CheckOutDate > checkInDate)
        .Select(res => res.RoomId)
        .Distinct()
        .ToListAsync();

            var availableRooms = await _context.Rooms // 'query' değişkenini ayırmana gerek yok
                      .Include(r => r.Translations) // <-- 1. EKSİK: Çevirileri dahil et
                      .Where(r => r.IsAvailable && r.Capacity >= guestCount)
              .Where(r => !bookedRoomIds.Contains(r.Id))
              .ToListAsync();

            // --- 2. EKSİK: Çeviriyi doldurma mantığı ---
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
            // --- Bitti ---

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

            // 2. Varsayılan dil (Eğer Almanca çeviri yoksa Türkçe göster)
            var defaultCulture = "tr-TR";

            // 3. Odaları, ilişkili tüm çevirileriyle birlikte veritabanından çek
            var rooms = await _context.Rooms
                .Include(r => r.Translations) // Çevirileri de sorguya dahil et
                .Where(r => r.IsAvailable)
                .ToListAsync();

            // 4. ÇEVİRİLERİ DÜZLEŞTİR (Flattening)
            // Bu döngü, her odanın [NotMapped] RoomType ve Description alanlarını
            // o anki dile göre doldurur.
            foreach (var room in rooms)
            {
                // Önce mevcut dili (örn: Almanca) aramayı dene
                var translation = room.Translations
                                      .FirstOrDefault(t => t.LanguageCode == currentCulture);

                // Eğer mevcut dilde (Almanca) çeviri bulunamazsa,
                // varsayılan dili (Türkçe) aramayı dene
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
                    // Acil durum metni (Bu normalde görünmemeli)
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
              .Include(r => r.Amenities)
              .Include(r => r.Translations) // <-- 1. EKSİK: Çevirileri dahil et
                      .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            // --- 2. EKSİK: Çeviriyi doldurma mantığı ---
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";

            var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                    ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);
            if (translation != null)
            {
                room.RoomType = translation.RoomType;
                room.Description = translation.Description;
            }
            // --- Bitti ---

            return View(room);
        }
    }
}
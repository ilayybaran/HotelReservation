
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            if (checkInDate >= checkOutDate)
            {
                TempData["SearchError"] = "Çıkış tarihi, giriş tarihinden sonra olmalıdır.";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Rooms
                .Where(r => r.IsAvailable && r.Capacity >= guestCount);

            // Bir odanın çakışması için: (Rez. Başlangıcı < Arama Sonu) VE (Rez. Sonu > Arama Başlangıcı)
            var bookedRoomIds = await _context.Reservations
                .Where(res => res.CheckInDate < checkOutDate && res.CheckOutDate > checkInDate)
                .Select(res => res.RoomId)
                .Distinct()
                .ToListAsync();

            // Kapasitesi uyan odalardan, rezerve edilmiş olanları çıkar.
            var availableRooms = await query
                .Where(r => !bookedRoomIds.Contains(r.Id))
                .ToListAsync();
 
            ViewBag.CheckInDate = checkInDate;
            ViewBag.CheckOutDate = checkOutDate;
            ViewBag.GuestCount = guestCount;
            ViewBag.ResultsCount = availableRooms.Count;

            return View("SearchResults", availableRooms);
        }

        // GET: /Rooms
        public async Task<IActionResult> Index()
        {
            // Sadece müsait olan odaları listele
            var availableRooms = await _context.Rooms.Where(r => r.IsAvailable).ToListAsync();
            return View(availableRooms);
        }

        // GET: /Rooms/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Amenities)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
    }
}

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

            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
    }
}
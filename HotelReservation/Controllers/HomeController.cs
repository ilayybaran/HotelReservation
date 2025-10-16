using HotelReservation.Data; 
using HotelReservation.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Diagnostics;
using System.Threading.Tasks; 

namespace HotelReservation.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // 1. Adım: Veritabanı bağlantısını Constructor ile alıyoruz.
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanından müsait olan odalardan ilk 3 tanesi
            var featuredRooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .Take(3)
                .ToListAsync();

            // 3. Adım: Bulduğumuz oda listesini View'a model olarak gönderiyoruz.
            return View(featuredRooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
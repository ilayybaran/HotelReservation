using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Linq; 
using System.Threading.Tasks; 

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsAdminController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationsAdminController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var defaultCulture = "tr-TR"; // Admin paneli için varsayılan dil

            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room) 
                    .ThenInclude(room => room.Translations) 
                .OrderByDescending(r => r.ReservationDate) 
                .ToListAsync();

            foreach (var res in reservations)
            {
                if (res.Room != null)
                {
                    var translation = res.Room.Translations
                        .FirstOrDefault(t => t.LanguageCode == defaultCulture);

                    if (translation != null)
                    {
                        // View'ın kullanabilmesi için C# özelliğini doldur
                        res.Room.RoomType = translation.RoomType;
                    }
                    else
                    {
                        res.Room.RoomType = "[Çeviri Yok]";
                    }
                }
            }
            return View(reservations);
        }

    }
}
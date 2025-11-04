using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsAdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/ReservationsAdmin
        public async Task<IActionResult> Index(string statusFilter) // Bir filtre parametresi ekle
        {
            var query = _context.Reservations
                            .Include(r => r.Room)
                            .Include(r => r.User) // Kullanıcıyı da dahil et
                            .AsQueryable();

            // Eğer URL'den bir filtre geldiyse (örn: ?statusFilter=PendingPayment)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(r => r.Status == statusFilter);
                ViewData["FilterTitle"] = $"Durumu '{statusFilter}' Olan Rezervasyonlar";
            }

            var reservations = await query.OrderByDescending(r => r.ReservationDate).ToListAsync();

            return View(reservations);
        }
    }
}
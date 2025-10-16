
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Controllers
{
    [Authorize] 
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations/List
        // Kullanıcının kendi rezervasyonlarını listeler
        public async Task<IActionResult> List()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userReservations = await _context.Reservations
                .Where(r => r.UserId == currentUser.Id)
                .Include(r => r.Room) // Oda bilgilerini de getirmek için
                .OrderByDescending(r => r.CheckInDate) // En yeniden eskie doğru sıralar
                .ToListAsync();

            return View(userReservations);
        }


        // GET: Reservations/Create?roomId=5  (Kullanıcıya formu gösterir)
        public async Task<IActionResult> Create(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || !room.IsAvailable)
            {
                return NotFound("Bu oda rezervasyona uygun değil veya bulunamadı.");
            }

            var reservationModel = new Reservation
            {
                RoomId = room.Id,
                Room = room,
                CheckInDate = System.DateTime.Today.AddDays(1),
                CheckOutDate = System.DateTime.Today.AddDays(2)
            };

            return View(reservationModel);
        }

        // POST: Reservations/Create (Kullanıcının doldurduğu formu alır)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            // Room nesnesini post işleminde kaybettiğimiz için veritabanından tekrar yüklüyoruz.
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            reservation.Room = room;

            // Çıkış tarihinin giriş tarihinden sonra olduğunu kontrol et
            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Çıkış tarihi, giriş tarihinden sonra olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                reservation.UserId = currentUser.Id;

                int numberOfNights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
                reservation.TotalPrice = numberOfNights * reservation.Room.PricePerNight;
                reservation.ReservationDate = System.DateTime.Now;

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla oluşturuldu!";
                return RedirectToAction(nameof(List));
            }

            // Eğer model geçerli değilse, formu hatalarla birlikte tekrar göster
            return View(reservation);
        }
    }
}
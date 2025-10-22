using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        // GET: Rezervasyon onay formu
        public async Task<IActionResult> Create(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Oda bulunamadı.";
                return RedirectToAction("Index", "Rooms");
            }

            if (checkInDate < DateTime.Today || checkOutDate <= checkInDate)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir tarih aralığı seçiniz.";
                return RedirectToAction("Index", "Rooms");
            }

            // Oda belirtilen tarihlerde müsait mi?
            bool isRoomAvailable = !await _context.Reservations.AnyAsync(r =>
                r.RoomId == roomId &&
                r.CheckInDate < checkOutDate &&
                r.CheckOutDate > checkInDate);

            if (!isRoomAvailable)
            {
                TempData["ErrorMessage"] = "Bu oda seçilen tarihlerde dolu.";
                return RedirectToAction("Index", "Rooms");
            }

            var reservation = new Reservation
            {
                RoomId = roomId,
                Room = room,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                UserId = _userManager.GetUserId(User),
                TotalPrice = (checkOutDate - checkInDate).Days * room.PricePerNight
            };

            return View(reservation);
        }

        // POST: Rezervasyonu Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,CheckInDate,CheckOutDate")] Reservation reservation)
        {
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Oda bulunamadı.";
                return RedirectToAction("Index", "Rooms");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı girişi bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                TempData["ErrorMessage"] = "Çıkış tarihi, giriş tarihinden sonra olmalıdır.";
                return RedirectToAction("Index", "Rooms");
            }

            // Aynı tarihlerde oda dolu mu
            bool isRoomAvailable = !await _context.Reservations.AnyAsync(r =>
                r.RoomId == reservation.RoomId &&
                r.CheckInDate < reservation.CheckOutDate &&
                r.CheckOutDate > reservation.CheckInDate);

            if (!isRoomAvailable)
            {
                TempData["ErrorMessage"] = "Üzgünüz, bu oda seçilen tarihlerde dolu.";
                return RedirectToAction("Index", "Rooms");
            }

            // Her şey yolundaysa kaydet
            reservation.UserId = user.Id;
            reservation.ReservationDate = DateTime.Now;
            reservation.TotalPrice = (reservation.CheckOutDate - reservation.CheckInDate).Days * room.PricePerNight;

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla oluşturuldu!";
            return RedirectToAction(nameof(ResList));
        }

        
        public async Task<IActionResult> ResList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var reservations = await _context.Reservations
                .Where(r => r.UserId == currentUser.Id)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CheckInDate)
                .ToListAsync();

            return View(reservations);
        }
    }
}

using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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
            var room = await _context.Rooms
            .Include(r => r.Translations) // Oda çevirileri
            .Include(r => r.Amenities)
            .ThenInclude(a => a.Translations) // Olanakların çevirileri
            .FirstOrDefaultAsync(r => r.Id == roomId);
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

            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";

            // Oda'yı çevir
            var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                    ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);
            if (translation != null)
            {
                room.RoomType = translation.RoomType;
                room.Description = translation.Description;
            }
            
            // Olanakları (Amenities) da çevir
            foreach (var amenity in room.Amenities)
            {
                var amenityTranslation = amenity.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                                ?? amenity.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);

                if (amenityTranslation != null)
                {
                    amenity.Name = amenityTranslation.Name; // Amenity'nin adını çevrilmiş metinle değiştir
                }
            }

            var reservation = new Reservation
            {
                RoomId = roomId,
                Room = room,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                UserId = _userManager.GetUserId(User),
                TotalPrice = (checkOutDate - checkInDate).Days * room.PricePerNight,
                Status = "PendinglPayment"
                
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
            reservation.Status = "PendingPayment";
            reservation.TotalPrice = (reservation.CheckOutDate - reservation.CheckInDate).Days * room.PricePerNight;

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla oluşturuldu!";
            return RedirectToAction(nameof(ResList));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAndPay([Bind("RoomId,CheckInDate,CheckOutDate")] Reservation reservation)
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
            bool isRoomAvailable = !await _context.Reservations.AnyAsync(r =>
                r.RoomId == reservation.RoomId &&
                r.CheckInDate < reservation.CheckOutDate &&
                r.CheckOutDate > reservation.CheckInDate);
            if (!isRoomAvailable)
            {
                TempData["ErrorMessage"] = "Üzgünüz, bu oda seçilen tarihlerde dolu.";
                return RedirectToAction("Index", "Rooms");
            }
            

            reservation.UserId = user.Id;
            reservation.ReservationDate = DateTime.Now;
            reservation.Status = "PendingPayment";
            reservation.TotalPrice = (reservation.CheckOutDate - reservation.CheckInDate).Days * room.PricePerNight;

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Payment", "Payment", new { reservationId = reservation.Id });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int reservationId)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }


            var reservation = await _context.Reservations
                                        .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == currentUser.Id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "İptal edilecek rezervasyon bulunamadı veya bu rezervasyon size ait değil.";
                return RedirectToAction("ResList", "Reservations", new { area = "" });
            }

            try
            {
                // 4. Rezervasyonu veritabanından sil
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync(); // Değişiklikleri kaydet

                TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla iptal edildi.";
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Rezervasyon iptal edilirken bir hata oluştu.";
            }

            // 5. Kullanıcıyı tekrar rezervasyon listesi sayfasına yönlendir
            return RedirectToAction("ResList", "Reservations", new { area = "" });
        }
    }
}


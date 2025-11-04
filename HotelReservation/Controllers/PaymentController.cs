using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Services; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace HotelReservation.Controllers
{
    [Authorize] // Sadece giriş yapanlar ödeme yapabilir
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyService _currencyService;

        public PaymentController(AppDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null || reservation.Status != "PendingPayment")
            {
                TempData["ErrorMessage"] = "Ödeme için geçerli bir rezervasyon bulunamadı.";
                return RedirectToAction("ResList", "Reservations");
            }

            var room = await _context.Rooms.FindAsync(reservation.RoomId);

            // Fiyatı kullanıcının seçtiği para birimine dönüştür
            var targetCurrency = Request.Cookies["UserCurrency"] ?? "TRY";
            var convertedPrice = await _currencyService.ConvertFromTRYAsync(reservation.TotalPrice, targetCurrency);

            var model = new PaymentViewModel
            {
                ReservationId = reservationId,
                Amount = convertedPrice,
                RoomType = room?.RoomType,
                Currency = targetCurrency
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Formda hata varsa (Required, Range vb.) formu tekrar göster
                return View("Payment", model);
            }

            await Task.Delay(2000);

            // Başarısızlık senaryosu 
            if (model.CVC == null)
            {
                ModelState.AddModelError("", "Banka tarafından ödeme reddedildi.");
                return View("Payment", model);
            }

            
            var reservation = await _context.Reservations.FindAsync(model.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Confirmed"; 
            reservation.PaymentDate = DateTime.UtcNow; // Ödeme tarihini kaydet
            _context.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rezervasyonunuz (ID: {reservation.Id}) başarıyla onaylandı.";
            return RedirectToAction("ResList", "Reservations");
        }
    }
}
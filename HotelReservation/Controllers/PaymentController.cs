using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Services; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Localization;

namespace HotelReservation.Controllers
{
    [Authorize] 
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

            var room = await _context.Rooms
                .Include(r=>r.Translations)
                .FirstOrDefaultAsync(r=>r.Id == reservation.RoomId);

            
            var targetCurrency = Request.Cookies["UserCurrency"] ?? "TRY";
            var convertedPrice = await _currencyService.ConvertFromTRYAsync(reservation.TotalPrice, targetCurrency);
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";

            string displayRoomType = "[Oda Adı Bulunamadı]"; 
            if (room != null)
            {
                var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                               ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);
                if (translation != null)
                {
                    displayRoomType = translation.RoomType;
                }
            }
            var model = new PaymentViewModel
            {
                ReservationId = reservationId,
                Amount = convertedPrice,
                RoomType = displayRoomType,
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
                return View("Payment", model);
            }

            await Task.Delay(2000);

           
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
            reservation.PaymentDate = DateTime.UtcNow; 
            _context.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rezervasyonunuz (ID: {reservation.Id}) başarıyla onaylandı.";
            return RedirectToAction("ResList", "Reservations");
        }
    }
}
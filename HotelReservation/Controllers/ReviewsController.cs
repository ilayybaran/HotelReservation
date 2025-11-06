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
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

         public async Task<IActionResult> Create(int reservationId)
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == user.Id);
            if (reservation == null) return NotFound("Rezervasyon bulunamadı");

            if(reservation.Status != "Confirmed" || reservation.CheckOutDate > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Sadece tamamlanmış konaklamalar için rezervasyon yapılabilir.";
                return RedirectToAction("ResList", "Reservations");
            }
            bool alreadyReviewed = await _context.Reviews.AnyAsync(r=> r.ReservationId == reservationId);
            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "Bu konaklama için zaten yorum yapmışsınız";
                return RedirectToAction("ResList", "Reservations");
            }

            var review = new Review
            {
                ReservationId = reservationId,
                RoomId = reservation.RoomId,
                Room = reservation.Room,
            };
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Rating,Comment,RoomId,ReservationId")] Review review)
        {
            var user = await _userManager.GetUserAsync(User);

           
            review.UserId = user.Id;
            review.DatePosted = DateTime.Now;

            ModelState.Remove("User");
            ModelState.Remove("Room");
            ModelState.Remove("Reservation");
            ModelState.Remove("UserId");

            bool alreadyReviewed = await _context.Reviews.AnyAsync(r => r.ReservationId == review.ReservationId);
            if (alreadyReviewed)
            {
                ModelState.AddModelError("", "Bu konaklama için zaten yorum yapmışsınız");
            }

            if (ModelState.IsValid)
            {
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yorumunuz için teşekkür ederiz";
                return RedirectToAction("ResList", "Reservations");
            }

            review.Room = await _context.Rooms.FindAsync(review.RoomId);
            return View(review);
        }
    }
}

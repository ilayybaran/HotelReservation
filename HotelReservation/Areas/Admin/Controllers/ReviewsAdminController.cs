using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsAdminController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsAdminController(AppDbContext context)
        {
            _context = context;
        }
        // GET: /Admin/ReviewsAdmin
        public async Task<IActionResult> Index()
        {
            var defaultCulture = "tr-TR";

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Translations)
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();
            
            // 'Room' nesnelerinin [NotMapped] RoomType alanlarını doldur
            foreach (var review in reviews)
            {
                if (review.Room != null)
                {
                    if (review.Room.Translations != null)
                    {
                        var translation = review.Room.Translations
                            .FirstOrDefault(t => t.LanguageCode == defaultCulture);

                        if (translation != null)
                        {
                            review.Room.RoomType = translation.RoomType;
                        }
                        else
                        {
                            review.Room.RoomType = "[Çeviri Yok]";
                        }
                    }
                    else
                    {
                        review.Room.RoomType = "[Oda Çeviri İlişkisi Yok]";
                    }
                }
            
            }
            return View(reviews);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            // Odanın adını (TR) doldur
            if (review.Room != null)
            {
                var translation = await _context.RoomTranslations
                    .FirstOrDefaultAsync(t => t.RoomId == review.RoomId && t.LanguageCode == "tr-TR");
                if (translation != null) review.Room.RoomType = translation.RoomType;
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}


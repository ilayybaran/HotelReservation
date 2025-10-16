using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomsAdminController : Controller
    {
        private readonly AppDbContext _context;

        public RoomsAdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/RoomsAdmin
        // Bu metot, admin paneli için tüm odaları (müsait olan/olmayan) listeler.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        // GET: /Admin/RoomsAdmin/Details/5
        // Adminin bir odanın detaylarını görmesi için.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null) return NotFound();
            return View(room);
        }

        // GET: /Admin/RoomsAdmin/Create
        // Yeni oda ekleme formunu gösterir.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/RoomsAdmin/Create
        // Formdan gelen yeni oda bilgilerini veritabanına kaydeder.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: /Admin/RoomsAdmin/Edit/5
        // Mevcut bir odayı düzenleme formunu, odanın bilgileriyle dolu olarak getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: /Admin/RoomsAdmin/Edit/5
        // Formda güncellenen bilgileri veritabanına kaydeder.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: /Admin/RoomsAdmin/Delete/5
        // Bir odayı silmeden önce onay sayfasını gösterir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: /Admin/RoomsAdmin/Delete/5
        // Onaylandıktan sonra odayı veritabanından siler.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
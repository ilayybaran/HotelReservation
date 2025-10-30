using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomsAdminController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        // GET: /Admin/RoomsAdmin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null) return NotFound();

            return View(room);
        }

        public async Task<IActionResult> Create() 
        {
            ViewBag.AllAmenities = await _context.Amenities.ToListAsync();
            return View(new Room());
        }

        // POST: /Admin/RoomsAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,RoomType,Description,Capacity,PricePerNight,IsAvailable")] Room room,
            IFormFile? ImageFile,
            List<int> selectedAmenityIds)
        {

            if (!ModelState.IsValid)
                return View(room);
         
            if (ImageFile != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string roomPath = Path.Combine(wwwRootPath, "images/rooms");

                if (!Directory.Exists(roomPath))
                    Directory.CreateDirectory(roomPath);

                string filePath = Path.Combine(roomPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                room.ImageUrl = "/images/rooms/" + fileName;
            }

            if (selectedAmenityIds != null)
            {
                foreach (var amenityId in selectedAmenityIds)
                {
                    var amenity = await _context.Amenities.FindAsync(amenityId);
                    if (amenity != null)
                    {
                        room.Amenities.Add(amenity);
                    }
                }
            }

            _context.Add(room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Oda başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/RoomsAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Odayı, mevcut seçili olanaklarıyla birlikte çek
            var room = await _context.Rooms
                                     .Include(r => r.Amenities) // Odaya bağlı olanakları da yükle
                                     .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            // View'a, formda gösterilecek tüm olanakların listesini gönder
            ViewBag.AllAmenities = await _context.Amenities.ToListAsync();

            return View(room);
        }

        // POST: /Admin/RoomsAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,RoomType,Description,Capacity,PricePerNight,IsAvailable")] Room room,
            IFormFile? ImageFile,
            List<int> selectedAmenityIds)
        {
            ModelState.Remove("Reservations");

            if (id != room.Id) return NotFound();
            var roomToUpdate = await _context.Rooms
                                     .Include(r => r.Amenities)
                                     .FirstOrDefaultAsync(r => r.Id == id);
            if (roomToUpdate == null) return NotFound();

            if (!ModelState.IsValid) return View(room);

            var roomFromDb = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (roomFromDb == null) return NotFound();

            room.ImageUrl = roomFromDb.ImageUrl;

            if (ImageFile != null)
            {
                if (!string.IsNullOrEmpty(room.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, room.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                // Yeni resmi kaydet
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string roomPath = Path.Combine(wwwRootPath, "images/rooms");

                if (!Directory.Exists(roomPath))
                    Directory.CreateDirectory(roomPath);

                string filePath = Path.Combine(roomPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                room.ImageUrl = "/images/rooms/" + fileName;
            }
            roomToUpdate.Amenities.Clear(); // Önce mevcut tüm olanakları temizle
            if (selectedAmenityIds != null)
            {
                foreach (var amenityId in selectedAmenityIds) // Sonra formdan gelen seçili olanları ekle
                {
                    var amenity = await _context.Amenities.FindAsync(amenityId);
                    if (amenity != null)
                    {
                        roomToUpdate.Amenities.Add(amenity);
                    }
                }
            }
         
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Oda başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/RoomsAdmin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null) return NotFound();

            return View(room);
        }

        // POST: /Admin/RoomsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            if (!string.IsNullOrEmpty(room.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, room.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Oda başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

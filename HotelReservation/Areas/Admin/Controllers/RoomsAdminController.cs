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

        // Controllers/Admin/RoomsAdminController.cs

        // Controllers/Admin/RoomsAdminController.cs

        public async Task<IActionResult> Index()
        {
            var defaultCulture = "tr-TR"; // Admin panelinde varsayılan olarak TR adları görünsün

            // 1. Odaları, çevirileriyle birlikte çek
            var rooms = await _context.Rooms
                .Include(r => r.Translations) // ÇEVİRİLERİ DAHİL ET
                .ToListAsync();

            // 2. "Hayalet" alanları (RoomType) Türkçe çeviriyle doldur
            foreach (var room in rooms)
            {
                var translation = room.Translations
                                      .FirstOrDefault(t => t.LanguageCode == defaultCulture);

                if (translation != null)
                {
                    room.RoomType = translation.RoomType;
                }
                else
                {
                    // Eğer (bir şekilde) Türkçe çevirisi hiç girilmemişse
                    room.RoomType = "[Çeviri Girilmemiş]";
                }
            }

            // 3. Artık RoomType alanı dolu olan listeyi View'a gönder
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
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Olanakları (CheckBox'lar) doldurmak için ViewBag'e yolla
            ViewBag.AllAmenities = await _context.Amenities.ToListAsync();

            // Yeni, boş bir Room modeli gönder (Translations listesi boş olacak)
            var model = new Room();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Capacity,PricePerNight,IsAvailable")] Room room,
            List<RoomTranslation> Translations, // Çeviri listesi
            List<int> selectedAmenityIds, // Olanak listesi
            IFormFile? ImageFile)
        {
            ModelState.Remove("Reservations");
            ModelState.Remove("Amenities");
            ModelState.Remove("RoomType"); // [NotMapped] oldukları için validasyondan çıkar
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
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
                    room.Amenities = new List<Amenity>();
                    foreach (var amenityId in selectedAmenityIds)
                    {
                        var amenity = await _context.Amenities.FindAsync(amenityId);
                        if (amenity != null)
                        {
                            room.Amenities.Add(amenity);
                        }
                    }
                }

                room.Translations = Translations;

                _context.Add(room);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Oda başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllAmenities = await _context.Amenities.ToListAsync();
            return View(room);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Amenities)
                .Include(r => r.Translations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            ViewBag.AllAmenities = await _context.Amenities.ToListAsync();

            return View(room); // Dolu modeli View'a gönder
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, // URL'den
            List<RoomTranslation> Translations,
            List<int> selectedAmenityIds,
            IFormFile? ImageFile,

            int Capacity,
            decimal PricePerNight,
            bool IsAvailable)
        {
            // Veritabanından mevcut odayı (ilişkileriyle) bul
            var roomToUpdate = await _context.Rooms
                .Include(r => r.Amenities)
                .Include(r => r.Translations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (roomToUpdate == null) return NotFound();

            roomToUpdate.Capacity = Capacity;
            roomToUpdate.PricePerNight = PricePerNight;
            roomToUpdate.IsAvailable = IsAvailable;

            // 3. Resim Güncelle
            if (ImageFile != null)
            {
                if (!string.IsNullOrEmpty(roomToUpdate.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, roomToUpdate.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))

                        System.IO.File.Delete(oldImagePath);
                }

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
                roomToUpdate.ImageUrl = "/images/rooms/" + fileName; // "roomToUpdate" güncellenir
            }

            roomToUpdate.Amenities.Clear();
            if (selectedAmenityIds != null)
            {
                foreach (var amenityId in selectedAmenityIds)
                {
                    var amenity = await _context.Amenities.FindAsync(amenityId);
                    if (amenity != null) roomToUpdate.Amenities.Add(amenity);
                }
            }

            foreach (var formTranslation in Translations)
            {
                if (string.IsNullOrWhiteSpace(formTranslation.RoomType) &&
                    string.IsNullOrWhiteSpace(formTranslation.Description))
                {
                    continue; // Boşsa bu dili atla
                }

                var dbTranslation = roomToUpdate.Translations
                    .FirstOrDefault(t => t.LanguageCode == formTranslation.LanguageCode);

                if (dbTranslation != null)
                {
                    dbTranslation.RoomType = formTranslation.RoomType;
                    dbTranslation.Description = formTranslation.Description;
                }
                else
                {
                    roomToUpdate.Translations.Add(formTranslation);
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Kaydederken bir hata oluştu: " + ex.Message;
                ViewBag.AllAmenities = await _context.Amenities.ToListAsync();
                return View(roomToUpdate);
            }

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

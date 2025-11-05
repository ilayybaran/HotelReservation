using Microsoft.AspNetCore.Mvc;
using HotelReservation.Models;
using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class AmenitiesAdminController : Controller
    {
        private readonly AppDbContext _context;

        public AmenitiesAdminController(AppDbContext context)
        {
            _context = context;
        }
        // GET: /Admin/AmenitiesAdmin
        public async Task<IActionResult> Index()
        {
            var amenities = await _context.Amenities
                .Include(a => a.Translations) // Tüm çevirileri çek
                .ToListAsync();

           
            foreach (var amenity in amenities)
            {
                var translation = amenity.Translations.FirstOrDefault(t => t.LanguageCode == "tr-TR")
                               ?? amenity.Translations.FirstOrDefault(); // Varsa TR, yoksa ilk bulduğunu al

                if (translation != null)
                {
                    amenity.Name = translation.Name; 
                }
                else
                {
                    amenity.Name = "ÇEVİRİ EKSİK";
                }
            }

            return View(amenities); 
        }
      
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
                    string IconClass,
                    string Name_tr, 
                    string Name_en, 
                    string Name_de 
                    )
        {
            
            if (string.IsNullOrEmpty(Name_tr) || string.IsNullOrEmpty(IconClass))
            {
                ModelState.AddModelError("", "Lütfen Türkçe Ad ve İkon Sınıfı alanlarını doldurun.");

                return View();
            }

            var newAmenity = new Amenity
            {
                IconClass = IconClass,
                Translations = new List<AmenityTranslation>() 
            };

           
            newAmenity.Translations.Add(new AmenityTranslation
            {
                LanguageCode = "tr-TR",
                Name = Name_tr
            });

            // İngilizce çeviriyi ekle 
            if (!string.IsNullOrEmpty(Name_en))
            {
                newAmenity.Translations.Add(new AmenityTranslation
                {
                    LanguageCode = "en-US",
                    Name = Name_en
                });
            }

            // Almanca çeviriyi ekle 
            if (!string.IsNullOrEmpty(Name_de))
            {
                newAmenity.Translations.Add(new AmenityTranslation
                {
                    LanguageCode = "de-DE",
                    Name = Name_de
                });
            }

            _context.Amenities.Add(newAmenity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Olanak başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Admin/AmenitiesAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Olanağı, ilişkili tüm çevirileriyle birlikte çek
            var amenity = await _context.Amenities
                .Include(a => a.Translations)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (amenity == null) return NotFound();

            return View(amenity);
        }

        // POST: /Admin/AmenitiesAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string IconClass, List<AmenityTranslation> Translations)
        {
            if (id <= 0) return NotFound();

            // Güncellenecek asıl olanağı, 'track' ederek (izleyerek) çek
            var amenityToUpdate = await _context.Amenities
                .Include(a => a.Translations)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (amenityToUpdate == null) return NotFound();

            
            amenityToUpdate.IconClass = IconClass;

            foreach (var incomingTranslation in Translations)
            {
                // Veritabanında bu dil için bir kayıt var mı?
                var existingTranslation = amenityToUpdate.Translations
                    .FirstOrDefault(t => t.LanguageCode == incomingTranslation.LanguageCode);

                if (existingTranslation != null)
                {
                    // Kayıt varsa, adını güncelle
                    existingTranslation.Name = incomingTranslation.Name;
                }
                else
                {
                    // Kayıt yoksa ve formdan bir isim geldiyse, yeni kayıt olarak ekle
                    if (!string.IsNullOrEmpty(incomingTranslation.Name))
                    {
                        amenityToUpdate.Translations.Add(new AmenityTranslation
                        {
                            LanguageCode = incomingTranslation.LanguageCode,
                            Name = incomingTranslation.Name,
                            AmenityId = id
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Olanak başarıyla güncellendi.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Olanak güncellenirken bir hata oluştu.";
                return View(amenityToUpdate);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}

        
    


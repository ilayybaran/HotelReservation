using HotelReservation.Data; 
using HotelReservation.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Diagnostics;
using System.Threading.Tasks; 

namespace HotelReservation.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // === BU KISIM DİL ÇEVİRİSİ İÇİN GEREKLİ ===
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var currentCulture = cultureFeature.RequestCulture.UICulture.Name;
            var defaultCulture = "tr-TR";

            // Odaları (örn: ilk 3 odayı) çevirileriyle birlikte çek
            var rooms = await _context.Rooms
                .Include(r => r.Translations) // Çevirileri dahil et
                .Where(r => r.IsAvailable)
                .Take(3) // Anasayfada sadece 3 tane göster (isteğe bağlı)
                .ToListAsync();

            // "Hayalet" alanları (RoomType, Description) dile göre doldur
            foreach (var room in rooms)
            {
                var translation = room.Translations.FirstOrDefault(t => t.LanguageCode == currentCulture)
                                ?? room.Translations.FirstOrDefault(t => t.LanguageCode == defaultCulture);

                if (translation != null)
                {
                    room.RoomType = translation.RoomType;
                    room.Description = translation.Description;
                }
                else
                {
                    room.RoomType = "[Çeviri Bulunamadı]";
                    room.Description = "[Açıklama Yok]";
                }
            }
            return View(rooms);
        }

            public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                // Dil seçimini 1 yıl boyunca 
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // Kullanıcıyı aynı sayfaya geri yönlendir
            return LocalRedirect(returnUrl);
        }
        [HttpPost]
        public IActionResult SetCurrency(string currencyCode, string returnUrl)
        {
            var supported = new[] { "TRY", "USD", "EUR" };
            if (supported.Contains(currencyCode))
            {
                Response.Cookies.Append(
                    "UserCurrency",
                    currencyCode,   
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax, Path="/" }
                );
            }

            return LocalRedirect(returnUrl);
        }
    }
}
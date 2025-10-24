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

        // 1. Adım: Veritabanı bağlantısını Constructor ile alıyoruz.
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanından müsait olan odalardan ilk 3 tanesi
            var featuredRooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .Take(3)
                .ToListAsync();

            // 3. Adım: Bulduğumuz oda listesini View'a model olarak gönderiyoruz.
            return View(featuredRooms);
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
            // Desteklenen bir para birimi olup olmadığını kontrol edebiliriz (isteğe bağlı)
            var supported = new[] { "TRY", "USD", "EUR" };
            if (supported.Contains(currencyCode))
            {
                Response.Cookies.Append(
                    "UserCurrency", // Cookie adı
                    currencyCode,   // Seçilen para birimi kodu (örn: "USD")
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax }
                );
            }

            return LocalRedirect(returnUrl);
        }
    }
}
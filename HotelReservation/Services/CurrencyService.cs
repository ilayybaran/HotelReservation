// Services/CurrencyService.cs
using Microsoft.Extensions.Caching.Memory; // Cache için
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;


namespace HotelReservation.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMemoryCache _memoryCache;
        private const string BaseCurrency = "TRY";
        private const string CacheKey = "_ExchangeRates";

        public CurrencyService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<decimal> ConvertFromTRYAsync(decimal amount, string targetCurrency)
        {
            if (string.IsNullOrEmpty(targetCurrency) || targetCurrency == BaseCurrency)
            {
                return amount; // Hedef TRY ise veya boşsa çevirme
            }

            var rates = await GetExchangeRatesAsync();

            if (rates.TryGetValue(targetCurrency, out decimal rate))
            {
                // TRY'den hedefe çevirme (1 TRY = X Target)
                // Dikkat: Kurlar genellikle ters verilir (1 USD = Y TRY).
                // API'den alırken bu mantığı tersine çevirmeniz gerekebilir.
                // Bizim simülasyonumuzda 1 TRY'nin X USD/EUR değeri olduğunu varsayıyoruz.
                return amount * rate;
            }

            // Kur bulunamazsa orijinal miktarı geri dön (veya hata logla)
            Console.WriteLine($"Uyarı: {targetCurrency} için kur bulunamadı.");
            return amount;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
        {
            // Önce cache'e bak
            if (_memoryCache.TryGetValue(CacheKey, out Dictionary<string, decimal> rates))
            {
                return rates;
            }

            // Cache'de yoksa, SİMÜLE EDİLMİŞ kurları al (GERÇEKTE BURADA API ÇAĞRISI OLUR)
            await Task.Delay(10); // Sahte bir gecikme
            rates = new Dictionary<string, decimal>
        {
            // 1 TRY = X Döviz (Bu değerleri güncel tutmalısınız)
            { "USD", 0.030m },
            { "EUR", 0.028m }
            // Diğer kurlar...
        };

            // Cache'e kaydet (1 saat geçerli)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            _memoryCache.Set(CacheKey, rates, cacheEntryOptions);

            return rates;
        }

        // Para birimi koduna göre CultureInfo döndürür (Formatlama için)
        public CultureInfo GetCultureInfoForCurrency(string currencyCode)
        {
            return currencyCode switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("de-DE"), // Veya fr-FR vb.
                "TRY" => new CultureInfo("tr-TR"),
                _ => CultureInfo.CurrentCulture, // Bulunamazsa varsayılan
            };
        }
    }
}
// Services/CurrencyService.cs
using Microsoft.Extensions.Caching.Memory; 
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
                return amount * rate;
            }

            // Kur bulunamazsa orijinal miktarı geri dön (veya hata logla)
            Console.WriteLine($"Uyarı: {targetCurrency} için kur bulunamadı.");
            return amount;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
        {
            
            if (_memoryCache.TryGetValue(CacheKey, out Dictionary<string, decimal> rates))
            {
                return rates;
            }

          
            await Task.Delay(10); // Sahte bir gecikme
            rates = new Dictionary<string, decimal>
        {
           
            { "USD", 0.024m },
            { "EUR", 0.020m }
            
        };

           
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
                "EUR" => new CultureInfo("de-DE"), 
                "TRY" => new CultureInfo("tr-TR"),
                _ => CultureInfo.CurrentCulture, // Bulunamazsa varsayılan
            };
        }
    }
}
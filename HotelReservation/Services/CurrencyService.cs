using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelReservation.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BaseCurrency = "TRY";
        private const string CacheKey = "_ExchangeRates";
        private const string ApiUrl = "https://api.frankfurter.app/latest?from=TRY";

        public CurrencyService(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> ConvertFromTRYAsync(decimal amount, string targetCurrency)
        {
            if (string.IsNullOrEmpty(targetCurrency) || targetCurrency == BaseCurrency)
                return amount;

            var rates = await GetExchangeRatesAsync();

            if (rates.TryGetValue(targetCurrency, out decimal rate))
                return amount * rate;

            Console.WriteLine($"⚠️ Uyarı: {targetCurrency} için kur bulunamadı.");
            return amount;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
        {
            if (_memoryCache.TryGetValue(CacheKey, out Dictionary<string, decimal> cachedRates))
                return cachedRates;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync(ApiUrl);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (!root.TryGetProperty("rates", out var ratesProp))
                    throw new Exception("API yanıtında 'rates' bulunamadı.");

                var rates = new Dictionary<string, decimal>();
                foreach (var rate in ratesProp.EnumerateObject())
                    rates[rate.Name] = rate.Value.GetDecimal();

                _memoryCache.Set(CacheKey, rates,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(6)));

                return rates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Kur bilgileri alınamadı: {ex.Message}");
                if (_memoryCache.TryGetValue(CacheKey, out Dictionary<string, decimal> fallbackRates))
                    return fallbackRates;

                return new Dictionary<string, decimal> { { BaseCurrency, 1m } };
            }
        }
        public CultureInfo GetCultureInfoForCurrency(string currencyCode)
        {
            return currencyCode switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("de-DE"),
                "TRY" => new CultureInfo("tr-TR"),
                _ => CultureInfo.CurrentCulture,
            };
        }
    }
}

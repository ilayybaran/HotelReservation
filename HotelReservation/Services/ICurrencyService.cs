using System.Threading.Tasks;

namespace HotelReservation.Services
{
    public interface ICurrencyService
    {
        // Belirli bir miktarı TRY'den hedef para birimine çevirir
        Task<decimal> ConvertFromTRYAsync(decimal amount, string targetCurrency);
        // Hedef para birimine göre CultureInfo döndürür (formatlama için)
        System.Globalization.CultureInfo GetCultureInfoForCurrency(string currencyCode);
    }
}
using HotelReservation.Areas.Admin.Models;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfToday = today.AddDays(1);

            // 1. İstatistik Kartlarını Hesapla
            viewModel.TotalRevenue = await _context.Reservations
                .Where(r => r.Status == "Confirmed")
                .SumAsync(r => r.TotalPrice);

            viewModel.MonthlyRevenue = await _context.Reservations
                .Where(r => r.Status == "Confirmed" && r.ReservationDate >= startOfMonth)
                .SumAsync(r => r.TotalPrice);

            viewModel.NewBookingsToday = await _context.Reservations
                .CountAsync(r => r.ReservationDate >= today && r.ReservationDate < endOfToday);

            viewModel.PendingBookings = await _context.Reservations
                .CountAsync(r => r.Status == "PendingPayment");

            // 2. Grafik Verisini Hesapla (Son 7 Günlük Rezervasyon Sayısı)
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayStart = date;
                var dayEnd = dayStart.AddDays(1);

                int count = await _context.Reservations
                    .CountAsync(r => r.ReservationDate >= dayStart && r.ReservationDate < dayEnd);

                // Grafiğe ekle (Örn: "31 Eki")
                viewModel.ChartLabels.Add(date.ToString("dd MMM", new CultureInfo("tr-TR")));
                viewModel.ChartData.Add(count);
            }

            return View(viewModel);
        }
    }
}
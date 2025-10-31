namespace HotelReservation.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int NewBookingsToday { get; set; }
        public int PendingBookings { get; set; }

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();
    }
}

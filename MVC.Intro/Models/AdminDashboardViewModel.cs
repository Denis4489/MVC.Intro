using System;
using System.Collections.Generic;

namespace MVC.Intro.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalOrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public DayOrdersStat? PeakDay { get; set; }
        public List<DayOrdersStat> OrdersByDay { get; set; } = new();
        public List<OrderRecord> RecentOrders { get; set; } = new();
    }

    public class DayOrdersStat
    {
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("dd.MM.yyyy (dddd)", new System.Globalization.CultureInfo("bg-BG"));
        public string ShortDate => Date.ToString("dd.MM (ddd)", new System.Globalization.CultureInfo("bg-BG"));
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int TicketsCount { get; set; }
        public bool IsPeakDay { get; set; }
        public double PercentageOfPeak { get; set; }
    }
}

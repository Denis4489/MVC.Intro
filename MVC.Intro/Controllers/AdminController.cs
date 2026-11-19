using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var totalTickets = orders.Sum(o => o.TotalItems);

            // Group orders by date (ignoring time)
            var byDayGroups = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DayOrdersStat
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount),
                    TicketsCount = g.Sum(o => o.TotalItems)
                })
                .ToList();

            // Find the peak day (day with most orders)
            var maxOrdersOnAnyDay = byDayGroups.Any() ? byDayGroups.Max(d => d.OrderCount) : 0;
            var peakDay = byDayGroups
                .OrderByDescending(d => d.OrderCount)
                .ThenByDescending(d => d.Date)
                .FirstOrDefault();

            if (peakDay != null)
            {
                peakDay.IsPeakDay = true;
            }

            // Calculate percentage of peak for progress bars and mark the peak day
            foreach (var stat in byDayGroups)
            {
                stat.PercentageOfPeak = maxOrdersOnAnyDay > 0
                    ? Math.Round(((double)stat.OrderCount / maxOrdersOnAnyDay) * 100, 1)
                    : 0;

                if (peakDay != null && stat.Date == peakDay.Date)
                {
                    stat.IsPeakDay = true;
                }
            }

            // Sort chronologically (most recent day first) for the daily table
            var orderedDays = byDayGroups
                .OrderByDescending(d => d.Date)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalOrdersCount = totalOrders,
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTickets,
                PeakDay = peakDay,
                OrdersByDay = orderedDays,
                RecentOrders = orders.Take(20).ToList()
            };

            return View(model);
        }
    }
}

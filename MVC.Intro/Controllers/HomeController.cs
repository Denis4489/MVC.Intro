using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventService _eventService;

        public HomeController(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var events = (await _eventService.GetAllEventsAsync())
                .Select(@event =>
                {
                    var catalog = EventCatalog.Get(@event.Name);
                    return new EventViewModel
                    {
                        Event = @event,
                        Category = catalog.Category,
                        Description = catalog.Description,
                        Accent = catalog.Accent
                    };
                })
                .ToList();

            return View(events);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };
            return View(model);
        }
    }
}

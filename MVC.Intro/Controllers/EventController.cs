using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    [Route("[controller]/[action]")]
    public class EventController : Controller
    {
        private readonly EventService _eventService;
        private readonly ImageService _imageService;

        public EventController(EventService eventService, ImageService imageService)
        {
            _eventService = eventService;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _eventService.GetAllEventsAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var viewModel = new CreateEventViewModel
            {
                AvailableImages = _imageService.GetAvailableImages()
            };
            return View(viewModel);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var viewModel = new CreateEventViewModel
            {
                Event = @event,
                AvailableImages = _imageService.GetAvailableImages()
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(CreateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableImages = _imageService.GetAvailableImages();
                return View("Edit", viewModel);
            }
            await _eventService.UpdateEventAsync(viewModel.Event);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableImages = _imageService.GetAvailableImages();
                return View("Create", viewModel);
            }
            await _eventService.AddEventAsync(viewModel.Event);
            return RedirectToAction(nameof(Index));
        }
    }
}

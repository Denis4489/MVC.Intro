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

        public IActionResult Index()
        {
            return View(_eventService.GetAllEvents());
        }
        [HttpGet("{id}")]
        public IActionResult Details(Guid id)
        {
            return View(_eventService.GetEventById(id));
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreateEventViewModel
            {
                AvailableImages = _imageService.GetAvailableImages()
            };
            return View(viewModel);
        }

        [HttpGet("{id}")]
        public IActionResult Edit(Guid id)
        {
            var @event = _eventService.GetEventById(id);
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
        public IActionResult EditEvent(CreateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableImages = _imageService.GetAvailableImages();
                return View("Edit", viewModel);
            }
            _eventService.UpdateEvent(viewModel.Event);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _eventService.DeleteEvent(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CreateEvent(CreateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableImages = _imageService.GetAvailableImages();
                return View("Create", viewModel);
            }
            _eventService.AddEvent(viewModel.Event);
            return RedirectToAction(nameof(Index));
        }
    }
}

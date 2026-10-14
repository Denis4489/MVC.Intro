using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class EventService
    {
        private const string EventPrefix = "EVT_";
        private readonly ILogger<EventService> _logger;
        private readonly AppDbContext _context;


        public EventService(ILogger<EventService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public List<Event> GetAllEvents()
        {
            return _context.Events.ToList();
        }
        public Event GetEventById(Guid id)
        {
            var @event = _context.Events.FirstOrDefault(e => e.Id == id);
            if (@event == null)
            {
                throw new ArgumentNullException("Event not found");
            }
            return @event;
        }

        public Event AddEvent(Event @event)
        {
            var toAdd = new Event
            {
                Name = @event.Name,
                Price = @event.Price,
                ImageUrl = @event.ImageUrl,
                EventDate = @event.EventDate,
                Venue = @event.Venue,
                TicketType = @event.TicketType,
                Capacity = @event.Capacity,
                AvailableTickets = @event.AvailableTickets,
                Description = @event.Description
            };
            _logger.LogInformation("Adding event: {EventName} with price {EventPrice}", toAdd.Name, toAdd.Price);
            if (@event == null)
            {
                throw new ArgumentNullException("Event is null");
            }
            toAdd.Id = Guid.NewGuid();
            if (!toAdd.Name.StartsWith(EventPrefix))
            {
                DecorateEventName(toAdd);
            }
            _context.Events.Add(toAdd);
            _context.SaveChanges();
            return toAdd;
        }
        public void DeleteEvent(Guid id)
        {
            var @event = _context.Events.FirstOrDefault(e => e.Id == id);
            if (@event == null)
            {
                throw new ArgumentNullException("Event not found");
            }
            _context.Events.Remove(@event);
            _context.SaveChanges();
        }

        public void UpdateEvent(Event @event)
        {
            var existing = _context.Events.FirstOrDefault(e => e.Id == @event.Id);
            if (existing == null)
            {
                throw new ArgumentNullException("Event not found");
            }
            existing.Name = @event.Name;
            existing.Price = @event.Price;
            existing.ImageUrl = @event.ImageUrl;
            existing.EventDate = @event.EventDate;
            existing.Venue = @event.Venue;
            existing.TicketType = @event.TicketType;
            existing.Capacity = @event.Capacity;
            existing.AvailableTickets = @event.AvailableTickets;
            existing.Description = @event.Description;
            _context.SaveChanges();
        }

        private void DecorateEventName(Event @event)
        {
            @event.Name = EventPrefix + @event.Name;
        }

    }
}

using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(Guid id)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event> AddEventAsync(Event @event)
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
            toAdd.Id = Guid.NewGuid();
            if (!toAdd.Name.StartsWith(EventPrefix))
            {
                DecorateEventName(toAdd);
            }
            _context.Events.Add(toAdd);
            await _context.SaveChangesAsync();
            return toAdd;
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null)
            {
                return;
            }
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event @event)
        {
            var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            if (existing == null)
            {
                return;
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
            await _context.SaveChangesAsync();
        }

        private void DecorateEventName(Event @event)
        {
            @event.Name = EventPrefix + @event.Name;
        }
    }
}

using System.Text.Json;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class CartService
    {
        private const string SessionKey = "TicketsCart";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EventService _eventService;

        public CartService(IHttpContextAccessor httpContextAccessor, EventService eventService)
        {
            _httpContextAccessor = httpContextAccessor;
            _eventService = eventService;
        }

        public List<CartItem> GetItems()
        {
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public int Count => GetItems().Sum(item => item.Quantity);

        public async Task AddAsync(Guid eventId, int quantity = 1)
        {
            var @event = await _eventService.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return;
            }
            var items = GetItems();
            var existing = items.FirstOrDefault(item => item.ProductId == eventId);
            if (existing is null)
            {
                items.Add(new CartItem { ProductId = eventId, Quantity = Math.Max(1, quantity) });
            }
            else
            {
                existing.Quantity += Math.Max(1, quantity);
            }

            Save(items);
        }

        public void Update(Guid eventId, int quantity)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(item => item.ProductId == eventId);
            if (existing is null)
            {
                return;
            }

            if (quantity <= 0)
            {
                items.Remove(existing);
            }
            else
            {
                existing.Quantity = quantity;
            }

            Save(items);
        }

        public void Remove(Guid eventId)
        {
            var items = GetItems().Where(item => item.ProductId != eventId).ToList();
            Save(items);
        }

        public void Clear() => Save(new List<CartItem>());

        public async Task<IReadOnlyList<CartLineViewModel>> GetLinesAsync()
        {
            var items = GetItems();
            var lines = new List<CartLineViewModel>();
            var validItems = new List<CartItem>();

            foreach (var item in items)
            {
                var @event = await _eventService.GetEventByIdAsync(item.ProductId);
                if (@event == null)
                {
                    continue;
                }
                var catalog = EventCatalog.Get(@event.Name);
                lines.Add(new CartLineViewModel
                {
                    ProductId = @event.Id,
                    Name = @event.Name,
                    Category = catalog.Category,
                    Price = @event.Price,
                    Quantity = item.Quantity
                });
                validItems.Add(item);
            }

            if (validItems.Count != items.Count)
            {
                Save(validItems);
            }

            return lines;
        }

        public async Task<decimal> GetTotalAsync() => (await GetLinesAsync()).Sum(l => l.LineTotal);

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Сесията не е налична.");

        private void Save(List<CartItem> items)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(items));
        }
    }
}

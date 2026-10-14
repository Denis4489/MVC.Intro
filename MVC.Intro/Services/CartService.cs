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

        public void Add(Guid eventId, int quantity = 1)
        {
            _eventService.GetEventById(eventId);
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

        public IReadOnlyList<CartLineViewModel> GetLines()
        {
            return GetItems()
                .Select(item =>
                {
                    var @event = _eventService.GetEventById(item.ProductId);
                    var catalog = EventCatalog.Get(@event.Name);
                    return new CartLineViewModel
                    {
                        ProductId = @event.Id,
                        Name = @event.Name,
                        Category = catalog.Category,
                        Price = @event.Price,
                        Quantity = item.Quantity
                    };
                })
                .ToList();
        }

        public decimal Total => GetLines().Sum(line => line.LineTotal);

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Сесията не е налична.");

        private void Save(List<CartItem> items)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(items));
        }
    }
}

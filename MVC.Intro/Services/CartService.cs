using System.Text.Json;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class CartService
    {
        private const string SessionKey = "PsgCart";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProductService _productService;

        public CartService(IHttpContextAccessor httpContextAccessor, ProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
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

        public void Add(Guid productId, int quantity = 1)
        {
            _productService.GetProductById(productId);
            var items = GetItems();
            var existing = items.FirstOrDefault(item => item.ProductId == productId);
            if (existing is null)
            {
                items.Add(new CartItem { ProductId = productId, Quantity = Math.Max(1, quantity) });
            }
            else
            {
                existing.Quantity += Math.Max(1, quantity);
            }

            Save(items);
        }

        public void Update(Guid productId, int quantity)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(item => item.ProductId == productId);
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

        public void Remove(Guid productId)
        {
            var items = GetItems().Where(item => item.ProductId != productId).ToList();
            Save(items);
        }

        public void Clear() => Save(new List<CartItem>());

        public IReadOnlyList<CartLineViewModel> GetLines()
        {
            return GetItems()
                .Select(item =>
                {
                    var product = _productService.GetProductById(item.ProductId);
                    var catalog = PsgCatalog.Get(product.Name);
                    return new CartLineViewModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Category = catalog.Category,
                        Price = product.Price,
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

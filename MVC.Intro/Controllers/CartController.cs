using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class CartController : Controller
    {
        private const string LastOrderSessionKey = "LastOrder";
        private readonly CartService _cartService;
        private readonly EventService _eventService;
        private readonly TicketPdfService _ticketPdfService;

        public CartController(CartService cartService, EventService eventService, TicketPdfService ticketPdfService)
        {
            _cartService = cartService;
            _eventService = eventService;
            _ticketPdfService = ticketPdfService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Total = await _cartService.GetTotalAsync();
            return View(await _cartService.GetLinesAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid id, string? returnUrl = null)
        {
            await _cartService.AddAsync(id);
            TempData["Notice"] = "Артикулът е добавен в количката.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Guid id, int quantity)
        {
            _cartService.Update(id, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(Guid id)
        {
            _cartService.Remove(id);
            TempData["Notice"] = "Артикулът е премахнат.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var json = HttpContext.Session.GetString(LastOrderSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction(nameof(Index));
            }

            var order = JsonSerializer.Deserialize<OrderConfirmationViewModel>(json);
            if (order == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string? note)
        {
            var lines = await _cartService.GetLinesAsync();
            if (!lines.Any())
            {
                TempData["Notice"] = "Количката е празна.";
                return RedirectToAction(nameof(Index));
            }

            var orderId = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            var customerName = User.Identity?.Name ?? "Клиент";
            var customerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            var order = new OrderConfirmationViewModel
            {
                OrderId = orderId,
                OrderDate = DateTime.Now,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                TotalAmount = await _cartService.GetTotalAsync(),
                TotalItems = _cartService.Count,
                Tickets = new List<TicketItemViewModel>()
            };

            int ticketIndex = 1;
            foreach (var line in lines)
            {
                var @event = await _eventService.GetEventByIdAsync(line.ProductId);
                var venue = string.IsNullOrWhiteSpace(@event?.Venue) ? "Централен стадион" : @event.Venue;
                var eventDate = @event?.EventDate ?? DateTime.Now.AddDays(7);
                var ticketType = string.IsNullOrWhiteSpace(@event?.TicketType) ? "Стандартен" : @event.TicketType;

                var ticketCode = $"{orderId}-{ticketIndex++:D2}";
                var qrPayload = $"=== TICKETS ARENA ===\n" +
                                $"Поръчка: #{orderId}\n" +
                                $"Билет код: {ticketCode}\n" +
                                $"Събитие: {line.Name}\n" +
                                $"Категория: {line.Category}\n" +
                                $"Място: {venue}\n" +
                                $"Дата: {eventDate:dd.MM.yyyy HH:mm} ч.\n" +
                                $"Тип билет: {ticketType}\n" +
                                $"Количество: {line.Quantity} бр.\n" +
                                $"Притежател: {customerName}\n" +
                                $"Сума: {line.LineTotal:0.00} €\n" +
                                $"Статус: ВАЛИДЕН ЗА ВХОД";

                var qrCodeBase64 = _ticketPdfService.GenerateQrCodeBase64(qrPayload);

                order.Tickets.Add(new TicketItemViewModel
                {
                    EventId = line.ProductId,
                    EventName = line.Name,
                    Category = line.Category,
                    Venue = venue,
                    EventDate = eventDate,
                    TicketType = ticketType,
                    Quantity = line.Quantity,
                    UnitPrice = line.Price,
                    TicketCode = ticketCode,
                    QrPayload = qrPayload,
                    QrCodeBase64 = qrCodeBase64
                });
            }

            // Save order in session for viewing and downloading
            HttpContext.Session.SetString(LastOrderSessionKey, JsonSerializer.Serialize(order));

            _cartService.Clear();
            return View(order);
        }

        [Authorize]
        [HttpGet]
        public IActionResult DownloadTicketPdf(string? orderId)
        {
            var order = GetOrderFromSession(orderId);
            if (order == null)
            {
                TempData["Notice"] = "Поръчката не е намерена.";
                return RedirectToAction(nameof(Index));
            }

            var pdfBytes = _ticketPdfService.GenerateTicketPdf(order);
            return File(pdfBytes, "application/pdf", $"Bilet-{order.OrderId}.pdf");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ViewTicketPdf(string? orderId)
        {
            var order = GetOrderFromSession(orderId);
            if (order == null)
            {
                TempData["Notice"] = "Поръчката не е намерена.";
                return RedirectToAction(nameof(Index));
            }

            var pdfBytes = _ticketPdfService.GenerateTicketPdf(order);
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"Bilet-{order.OrderId}.pdf\"");
            return File(pdfBytes, "application/pdf");
        }

        private OrderConfirmationViewModel? GetOrderFromSession(string? orderId)
        {
            var json = HttpContext.Session.GetString(LastOrderSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var order = JsonSerializer.Deserialize<OrderConfirmationViewModel>(json);
            if (order == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(orderId) && !string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return order;
        }
    }
}

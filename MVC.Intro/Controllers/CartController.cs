using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            if (_cartService.Count == 0)
            {
                TempData["Notice"] = "Количката е празна.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Total = await _cartService.GetTotalAsync();
            ViewBag.ItemCount = _cartService.Count;
            _cartService.Clear();
            return View();
        }
    }
}

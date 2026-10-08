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

        public IActionResult Index()
        {
            ViewBag.Total = _cartService.Total;
            return View(_cartService.GetLines());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Guid id, string? returnUrl = null)
        {
            _cartService.Add(id);
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
        public IActionResult Checkout()
        {
            if (_cartService.Count == 0)
            {
                TempData["Notice"] = "Количката е празна.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Total = _cartService.Total;
            ViewBag.ItemCount = _cartService.Count;
            _cartService.Clear();
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Services;

namespace MVC.Intro.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartBadgeViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            return View(_cartService.Count);
        }
    }
}

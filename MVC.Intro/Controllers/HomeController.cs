using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;

        public HomeController(ProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var products = _productService.GetAllProducts()
                .Select(product =>
                {
                    var catalog = PsgCatalog.Get(product.Name);
                    return new ShopProductViewModel
                    {
                        Product = product,
                        Category = catalog.Category,
                        Description = catalog.Description,
                        Accent = catalog.Accent
                    };
                })
                .ToList();

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

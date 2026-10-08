using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    [Route("[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ImageService _imageService;

        public ProductController(ProductService productService, ImageService imageService)
        {
            _productService = productService;
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View(_productService.GetAllProducts());
        }
        [HttpGet("{id}")]//https://localhost:7206/Product/Details/52e9ba06-3b56-4f3d-973c-388efb0e4417
        public IActionResult Details(Guid id)
        {
            return View(_productService.GetProductById(id));
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreateProductViewModel
            {
                AvailableImages = _imageService.GetAvailableImages()
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CreateProduct(CreateProductViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableImages = _imageService.GetAvailableImages();
                return View("Create", viewModel);
            }
            _productService.AddProduct(viewModel.Product);
            return RedirectToAction(nameof(Index));
        }
    }
}

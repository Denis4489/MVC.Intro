using Microsoft.AspNetCore.Mvc;

namespace MVC.Intro.Controllers
{
    public class RoutingController : Controller
    {
        public IActionResult Default()
        {
            return View();
        }
    }
}

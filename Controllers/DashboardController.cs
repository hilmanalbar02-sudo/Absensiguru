using Microsoft.AspNetCore.Mvc;

namespace Absensiguru.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Admin()
        {
            return View();
        }

        public IActionResult Guru()
        {
            return View();
        }

        public IActionResult TU()
        {
            return View();
        }
    }
}
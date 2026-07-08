using Microsoft.AspNetCore.Mvc;

namespace Absensiguru.Controllers
{
    public class GuruController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
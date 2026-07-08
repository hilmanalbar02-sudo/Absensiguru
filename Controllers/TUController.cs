using Microsoft.AspNetCore.Mvc;

namespace Absensiguru.Controllers
{
    public class TUController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
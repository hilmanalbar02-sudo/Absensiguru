using Microsoft.AspNetCore.Mvc;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // Menghitung jumlah seluruh guru
            ViewBag.TotalGuru = _context.Gurus.Count();

            return View();
        }
    }
}
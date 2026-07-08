using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // HALAMAN LOGIN
        // ==========================

        [HttpGet]
        public IActionResult Index()
        {
            // Jika sudah login, arahkan sesuai role
            var role = HttpContext.Session.GetString("Role");

            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                if (role == "Guru")
                    return RedirectToAction("Dashboard", "Guru");

                if (role == "TU")
                    return RedirectToAction("Dashboard", "TU");
            }

            return View();
        }

        // ==========================
        // PROSES LOGIN
        // ==========================

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Users
                               .Include(u => u.IdGuruNavigation)
                               .FirstOrDefault(u =>
                                    u.Username == username &&
                                    u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Username atau Password salah.";
                return View();
            }

            // ==========================
            // SIMPAN SESSION
            // ==========================

            HttpContext.Session.SetInt32("IdUser", user.IdUser);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.IdGuruNavigation != null)
            {
                HttpContext.Session.SetInt32("IdGuru", user.IdGuruNavigation.IdGuru);
                HttpContext.Session.SetString("NamaGuru", user.IdGuruNavigation.NamaGuru);
            }

            // ==========================
            // REDIRECT SESUAI ROLE
            // ==========================

            switch (user.Role)
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");

                case "Guru":
                    return RedirectToAction("Dashboard", "Guru");

                case "TU":
                    return RedirectToAction("Dashboard", "TU");

                default:
                    ViewBag.Error = "Role tidak dikenali.";
                    return View();
            }
        }

        // ==========================
        // LOGOUT
        // ==========================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }
}
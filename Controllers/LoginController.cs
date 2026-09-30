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
            // Menambahkan ThenInclude untuk memuat data Jabatan dari Guru
            var user = _context.Users
                               .Include(u => u.IdGuruNavigation)
                                   .ThenInclude(g => g.IdJabatanNavigation)
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

            if (user.IdGuru != null)
            {
                HttpContext.Session.SetInt32("IdGuru", user.IdGuru.Value);
                HttpContext.Session.SetString(
                    "NamaGuru",
                    user.IdGuruNavigation?.NamaGuru ?? ""
                );
                HttpContext.Session.SetString(
                    "Role",
                    user.Role
                );
                HttpContext.Session.SetString(
                    "Foto",
                    user.IdGuruNavigation?.Foto ?? ""
                );
                HttpContext.Session.SetString(
                    "Jabatan",
                    user.IdGuruNavigation?.IdJabatanNavigation?.NamaJabatan ?? "-"
                );
            }
            else
            {
                // Fallback jika user login bukan entitas Guru (misal Admin murni tanpa data Guru)
                HttpContext.Session.SetString("Role", user.Role);
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
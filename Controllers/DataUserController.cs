using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class DataUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // INDEX
        // ==========================

        public async Task<IActionResult> Index(string searchString)
        {
            var users = _context.Users
                .Include(u => u.IdGuruNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(x =>
                    x.Username.Contains(searchString) ||
                    (x.IdGuruNavigation != null &&
                     x.IdGuruNavigation.NamaGuru.Contains(searchString)));
            }

            ViewBag.Search = searchString;

            return View(await users
                .OrderBy(x => x.Username)
                .ToListAsync());
        }

        // ==========================
        // DETAIL
        // ==========================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users
                .Include(x => x.IdGuruNavigation)
                .FirstOrDefaultAsync(x => x.IdUser == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // ==========================
        // CREATE
        // ==========================

        public IActionResult Create()
        {
            ViewBag.Guru = _context.Gurus
                .OrderBy(x => x.NamaGuru)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            ModelState.Remove("IdGuruNavigation");

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                TempData["Success"] = "User berhasil ditambahkan.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guru = _context.Gurus
                .OrderBy(x => x.NamaGuru)
                .ToList();

            return View(user);
        }

        // ==========================
        // EDIT
        // ==========================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            ViewBag.Guru = _context.Gurus
                .OrderBy(x => x.NamaGuru)
                .ToList();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.IdUser)
                return NotFound();

            ModelState.Remove("IdGuruNavigation");

            if (ModelState.IsValid)
            {
                _context.Update(user);

                await _context.SaveChangesAsync();

                TempData["Success"] = "User berhasil diperbarui.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guru = _context.Gurus
                .OrderBy(x => x.NamaGuru)
                .ToList();

            return View(user);
        }

        // ==========================
        // DELETE
        // ==========================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users
                .Include(x => x.IdGuruNavigation)
                .FirstOrDefaultAsync(x => x.IdUser == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                TempData["Success"] = "User berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
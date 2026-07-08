using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class DataAbsensiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataAbsensiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // INDEX
        // ==========================

        public async Task<IActionResult> Index(string searchString)
        {
            var absensi = _context.Absensis
                .Include(a => a.IdGuruNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                absensi = absensi.Where(a =>
                    a.IdGuruNavigation.NamaGuru.Contains(searchString) ||
                    a.Status.Contains(searchString));
            }

            ViewBag.Search = searchString;

            return View(await absensi
                .OrderByDescending(a => a.Tanggal)
                .ThenBy(a => a.IdGuruNavigation.NamaGuru)
                .ToListAsync());
        }

        // ==========================
        // DETAILS
        // ==========================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var absensi = await _context.Absensis
                .Include(a => a.IdGuruNavigation)
                .FirstOrDefaultAsync(a => a.IdAbsensi == id);

            if (absensi == null)
                return NotFound();

            return View(absensi);
        }

        // ==========================
        // CREATE
        // ==========================

        public IActionResult Create()
        {
            ViewBag.Guru = new SelectList(
                _context.Gurus.OrderBy(g => g.NamaGuru),
                "IdGuru",
                "NamaGuru");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Absensi absensi)
        {
            ModelState.Remove("IdGuruNavigation");

            if (ModelState.IsValid)
            {
                _context.Absensis.Add(absensi);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Data absensi berhasil ditambahkan.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guru = new SelectList(
                _context.Gurus.OrderBy(g => g.NamaGuru),
                "IdGuru",
                "NamaGuru",
                absensi.IdGuru);

            return View(absensi);
        }

        // ==========================
        // EDIT
        // ==========================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var absensi = await _context.Absensis.FindAsync(id);

            if (absensi == null)
                return NotFound();

            ViewBag.Guru = new SelectList(
                _context.Gurus.OrderBy(g => g.NamaGuru),
                "IdGuru",
                "NamaGuru",
                absensi.IdGuru);

            return View(absensi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Absensi absensi)
        {
            if (id != absensi.IdAbsensi)
                return NotFound();

            ModelState.Remove("IdGuruNavigation");

            if (ModelState.IsValid)
            {
                _context.Update(absensi);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Data absensi berhasil diperbarui.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guru = new SelectList(
                _context.Gurus.OrderBy(g => g.NamaGuru),
                "IdGuru",
                "NamaGuru",
                absensi.IdGuru);

            return View(absensi);
        }

        // ==========================
        // DELETE
        // ==========================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var absensi = await _context.Absensis
                .Include(a => a.IdGuruNavigation)
                .FirstOrDefaultAsync(a => a.IdAbsensi == id);

            if (absensi == null)
                return NotFound();

            return View(absensi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var absensi = await _context.Absensis.FindAsync(id);

            if (absensi != null)
            {
                _context.Absensis.Remove(absensi);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Data absensi berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
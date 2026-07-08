using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class DataJabatanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataJabatanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DAFTAR JABATAN
        // ===========================

        public async Task<IActionResult> Index(string searchString)
        {
            var jabatan = _context.Jabatans.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                jabatan = jabatan.Where(j =>
                    j.NamaJabatan.Contains(searchString));
            }

            ViewBag.Search = searchString;

            return View(await jabatan
                .OrderBy(j => j.NamaJabatan)
                .ToListAsync());
        }

        // ===========================
        // DETAIL
        // ===========================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var jabatan = await _context.Jabatans
                .FirstOrDefaultAsync(x => x.IdJabatan == id);

            if (jabatan == null)
                return NotFound();

            return View(jabatan);
        }

        // ===========================
        // FORM TAMBAH
        // ===========================

        public IActionResult Create()
        {
            return View();
        }

        // ===========================
        // SIMPAN
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Jabatan jabatan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jabatan);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Data jabatan berhasil ditambahkan.";

                return RedirectToAction(nameof(Index));
            }

            return View(jabatan);
        }

        // ===========================
        // FORM EDIT
        // ===========================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var jabatan = await _context.Jabatans.FindAsync(id);

            if (jabatan == null)
                return NotFound();

            return View(jabatan);
        }

        // ===========================
        // SIMPAN EDIT
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Jabatan jabatan)
        {
            if (id != jabatan.IdJabatan)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(jabatan);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Data jabatan berhasil diperbarui.";

                return RedirectToAction(nameof(Index));
            }

            return View(jabatan);
        }

        // ===========================
        // HAPUS
        // ===========================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var jabatan = await _context.Jabatans
                .FirstOrDefaultAsync(x => x.IdJabatan == id);

            if (jabatan == null)
                return NotFound();

            return View(jabatan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jabatan = await _context.Jabatans.FindAsync(id);

            if (jabatan != null)
            {
                _context.Jabatans.Remove(jabatan);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Data jabatan berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
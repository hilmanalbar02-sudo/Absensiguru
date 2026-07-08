using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class DataGuruController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataGuruController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DAFTAR GURU + PENCARIAN
        // ==========================================

        public async Task<IActionResult> Index(string searchString)
        {
            var guru = _context.Gurus
                               .Include(g => g.IdJabatanNavigation)
                               .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                guru = guru.Where(g =>
                    g.NamaGuru.Contains(searchString) ||
                    g.Nip.Contains(searchString));
            }

            ViewBag.Search = searchString;

            return View(await guru
                .OrderBy(g => g.NamaGuru)
                .ToListAsync());
        }

        // ==========================================
        // DETAIL GURU
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var guru = await _context.Gurus
                                     .Include(g => g.IdJabatanNavigation)
                                     .FirstOrDefaultAsync(g => g.IdGuru == id);

            if (guru == null)
                return NotFound();

            return View(guru);
        }

        // ==========================================
        // FORM TAMBAH
        // ==========================================

        public IActionResult Create()
        {
            ViewBag.Jabatan = _context.Jabatans.ToList();
            return View();
        }

        // ==========================================
        // SIMPAN DATA
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guru guru)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("========== MODEL STATE ERROR ==========");

                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine($"{item.Key} : {error.ErrorMessage}");
                    }
                }

                ViewBag.Jabatan = _context.Jabatans.ToList();
                return View(guru);
            }

            try
            {
                _context.Gurus.Add(guru);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Data guru berhasil ditambahkan.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("========== DATABASE ERROR ==========");
                Console.WriteLine(ex.ToString());

                ViewBag.Jabatan = _context.Jabatans.ToList();
                return View(guru);
            }
        }

        // ==========================================
        // FORM EDIT
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var guru = await _context.Gurus.FindAsync(id);

            if (guru == null)
                return NotFound();

            ViewBag.Jabatan = _context.Jabatans.ToList();

            return View(guru);
        }

        // ==========================================
        // SIMPAN EDIT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Guru guru)
        {
            if (id != guru.IdGuru)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Jabatan = _context.Jabatans.ToList();
                return View(guru);
            }

            try
            {
                _context.Update(guru);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Data guru berhasil diperbarui.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gurus.Any(e => e.IdGuru == guru.IdGuru))
                    return NotFound();

                throw;
            }
        }

        // ==========================================
        // FORM HAPUS
        // ==========================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var guru = await _context.Gurus
                                     .Include(g => g.IdJabatanNavigation)
                                     .FirstOrDefaultAsync(g => g.IdGuru == id);

            if (guru == null)
                return NotFound();

            return View(guru);
        }

        // ==========================================
        // KONFIRMASI HAPUS
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guru = await _context.Gurus.FindAsync(id);

            if (guru != null)
            {
                _context.Gurus.Remove(guru);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Data guru berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
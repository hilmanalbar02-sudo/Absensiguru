using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;

namespace Absensiguru.Controllers
{
    public class DataIzinController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataIzinController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var izin = _context.Izins
                .Include(x => x.IdGuruNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                izin = izin.Where(x =>
                    x.IdGuruNavigation.NamaGuru.Contains(searchString) ||
                    x.JenisIzin.Contains(searchString));
            }

            ViewBag.Search = searchString;

            return View(await izin
                .OrderByDescending(x => x.Tanggal)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var izin = await _context.Izins
                .Include(x => x.IdGuruNavigation)
                .FirstOrDefaultAsync(x => x.IdIzin == id);

            if (izin == null)
                return NotFound();

            return View(izin);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var izin = await _context.Izins
                .Include(x => x.IdGuruNavigation)
                .FirstOrDefaultAsync(x => x.IdIzin == id);

            if (izin == null)
                return NotFound();

            return View(izin);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var izin = await _context.Izins.FindAsync(id);

            if (izin != null)
            {
                _context.Izins.Remove(izin);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Data izin berhasil dihapus.";

            return RedirectToAction(nameof(Index));
        }
    }
}
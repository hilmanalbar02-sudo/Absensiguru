using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Absensiguru.Controllers
{
    public class DataGuruController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Menyuntikkan IWebHostEnvironment untuk mendapatkan jalur absolut folder wwwroot
        public DataGuruController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        // DETAIL GURU (Asinkron & Aman)
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
        // SIMPAN DATA + UPLOAD FOTO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guru guru, IFormFile? FotoFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Jabatan = _context.Jabatans.ToList();
                return View(guru);
            }

            try
            {
                // Proses upload foto jika ada file yang dipilih
                if (FotoFile != null && FotoFile.Length > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "guru");
                    
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    // Membuat nama unik file untuk menghindari duplikasi
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FotoFile.CopyToAsync(fileStream);
                    }

                    // Simpan nama file ke properti model
                    guru.Foto = fileName;
                }

                _context.Gurus.Add(guru);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Data guru berhasil ditambahkan.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
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
        // SIMPAN EDIT + UPDATE FOTO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Guru guru, IFormFile? FotoFile)
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
                // Ambil data asli dari database secara AsNoTracking agar tidak conflict saat update
                var existingGuru = await _context.Gurus.AsNoTracking().FirstOrDefaultAsync(g => g.IdGuru == id);
                if (existingGuru == null)
                    return NotFound();

                if (FotoFile != null && FotoFile.Length > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "guru");
                    
                    // Hapus foto lama jika ada
                    if (!string.IsNullOrEmpty(existingGuru.Foto))
                    {
                        string oldFilePath = Path.Combine(uploadDir, existingGuru.Foto);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    // Upload foto baru
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FotoFile.CopyToAsync(fileStream);
                    }

                    guru.Foto = fileName;
                }
                else
                {
                    // Jika tidak memilih file baru, pertahankan nama foto lama
                    guru.Foto = existingGuru.Foto;
                }

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
        // KONFIRMASI HAPUS + HAPUS BERKAS FOTO
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guru = await _context.Gurus.FindAsync(id);

            if (guru != null)
            {
                // Hapus berkas foto fisik dari server folder wwwroot sebelum data dihapus dari DB
                if (!string.IsNullOrEmpty(guru.Foto))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "guru", guru.Foto);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Gurus.Remove(guru);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Data guru berhasil dihapus.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
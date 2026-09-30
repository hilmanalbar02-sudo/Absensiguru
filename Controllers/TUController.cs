using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;
using Absensiguru.ViewModels; // Tetap dipertahankan untuk ProfilGuruViewModel
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting; 
using System.IO; 

namespace Absensiguru.Controllers
{
    public class TUController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; 

        // Constructor
        public TUController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================
        // DASHBOARD TU
        // ==========================
        public IActionResult Dashboard()
        {
            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            // ==========================
            // STATISTIK
            // ==========================
            ViewBag.TotalGuru = _context.Gurus.Count();

            ViewBag.Hadir = _context.Absensis
                .Count(a => a.Tanggal == hariIni && a.Status == "Hadir");

            ViewBag.Izin = _context.Izins
                .Count(i => i.Tanggal == hariIni && i.Status == "Disetujui");

            ViewBag.Alpha = ViewBag.TotalGuru - (ViewBag.Hadir + ViewBag.Izin);

            if (ViewBag.Alpha < 0)
                ViewBag.Alpha = 0;

            // ==========================
            // MONITORING REALTIME (SEMUA GURU) - PERBAIKAN TOTAL
            // ==========================
            // 1. Tarik data mentah dari database ke memori server secara terpisah (sangat ringan & aman)
            var listGuru = _context.Gurus.ToList();
            var listAbsensi = _context.Absensis.Where(a => a.Tanggal == hariIni).ToList();
            var listIzin = _context.Izins.Where(i => i.Tanggal == hariIni && i.Status == "Disetujui").ToList();

            // 2. Gabungkan dan urutkan murni di dalam memori (Client-Side), bebas dari eror translasi SQL
            ViewBag.AbsensiHariIni = listGuru
                .Select(g => Tuple.Create(
                    g,
                    listAbsensi.FirstOrDefault(a => a.IdGuru == g.IdGuru),
                    listIzin.FirstOrDefault(i => i.IdGuru == g.IdGuru)
                ))
                .OrderBy(x => x.Item1.NamaGuru) // Pengurutan dilakukan setelah data aman menjadi objek memori
                .ToList();

            // ==========================
            // IZIN TERBARU
            // ==========================
            ViewBag.IzinTerbaru = _context.Izins
                .Include(i => i.IdGuruNavigation)
                .OrderByDescending(i => i.Tanggal)
                .Take(5)
                .ToList();

            return View();
        }

        // ==========================
        // ABSENSI SAYA
        // ==========================
        public IActionResult AbsensiSaya()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var guru = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .FirstOrDefault(g => g.IdGuru == idGuru);

            if (guru == null)
                return RedirectToAction(nameof(Dashboard));

            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            ViewBag.AbsensiHariIni = _context.Absensis
                .FirstOrDefault(a =>
                    a.IdGuru == idGuru &&
                    a.Tanggal == hariIni);

            return View(guru);
        }

        // ==========================
        // ABSEN MASUK
        // ==========================
        [HttpPost]
        public IActionResult AbsenMasuk()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var sekarang = DateTime.Now;

            if (sekarang.TimeOfDay > new TimeSpan(7, 30, 0))
            {
                TempData["Error"] = "Jam absen masuk sudah ditutup.";
                return RedirectToAction(nameof(AbsensiSaya));
            }

            var hariIni = DateOnly.FromDateTime(sekarang);

            var cek = _context.Absensis.FirstOrDefault(a =>
                a.IdGuru == idGuru &&
                a.Tanggal == hariIni);

            if (cek != null)
            {
                TempData["Error"] = "Anda sudah absen hari ini.";
                return RedirectToAction(nameof(AbsensiSaya));
            }

            _context.Absensis.Add(new Absensi
            {
                IdGuru = idGuru.Value,
                Tanggal = hariIni,
                JamMasuk = TimeOnly.FromDateTime(sekarang),
                Status = "Hadir",
                Keterangan = "Absen Masuk"
            });

            _context.SaveChanges();

            TempData["Sukses"] = "Absen masuk berhasil.";

            return RedirectToAction(nameof(AbsensiSaya));
        }

        // ==========================
        // ABSEN PULANG
        // ==========================
        [HttpPost]
        public IActionResult AbsenPulang()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var sekarang = DateTime.Now;

            if (sekarang.TimeOfDay < new TimeSpan(13, 0, 0))
            {
                TempData["Error"] = "Belum waktunya absen pulang.";
                return RedirectToAction(nameof(AbsensiSaya));
            }

            var hariIni = DateOnly.FromDateTime(sekarang);

            var absensi = _context.Absensis.FirstOrDefault(a =>
                a.IdGuru == idGuru &&
                a.Tanggal == hariIni);

            if (absensi == null)
            {
                TempData["Error"] = "Silakan absen masuk terlebih dahulu.";
                return RedirectToAction(nameof(AbsensiSaya));
            }

            if (absensi.JamPulang != null)
            {
                TempData["Error"] = "Anda sudah absen pulang.";
                return RedirectToAction(nameof(AbsensiSaya));
            }

            absensi.JamPulang = TimeOnly.FromDateTime(sekarang);

            _context.SaveChanges();

            TempData["Sukses"] = "Absen pulang berhasil.";

            return RedirectToAction(nameof(AbsensiSaya));
        }

        // ==========================
        // RIWAYAT SAYA
        // ==========================
        public IActionResult RiwayatSaya()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var riwayat = _context.Absensis
                .Where(a => a.IdGuru == idGuru)
                .OrderByDescending(a => a.Tanggal)
                .ToList();

            return View(riwayat);
        }

        // ==========================
        // PROFIL SAYA (GET)
        // ==========================
        [HttpGet]
        public IActionResult ProfilSaya()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var guru = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .FirstOrDefault(g => g.IdGuru == idGuru);

            if (guru == null)
                return RedirectToAction(nameof(Dashboard));

            var model = new ProfilGuruViewModel
            {
                IdGuru = guru.IdGuru,
                Nip = guru.Nip,
                NamaGuru = guru.NamaGuru,
                Jabatan = guru.IdJabatanNavigation?.NamaJabatan ?? "-",
                Email = guru.Email,
                NoHp = guru.NoHp,
                Alamat = guru.Alamat,
                Foto = guru.Foto
            };

            return View(model);
        }

        // ==========================
        // SIMPAN PROFIL (POST)
        // ==========================
        [HttpPost]
        public IActionResult ProfilSaya(ProfilGuruViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var guru = _context.Gurus.FirstOrDefault(g => g.IdGuru == model.IdGuru);

            if (guru == null)
                return RedirectToAction(nameof(Dashboard));

            guru.Email = model.Email;
            guru.NoHp = model.NoHp;
            guru.Alamat = model.Alamat;

            if (model.FotoBaru != null)
            {
                var folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "guru");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var namaFile = Guid.NewGuid().ToString() + Path.GetExtension(model.FotoBaru.FileName);
                var path = Path.Combine(folder, namaFile);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    model.FotoBaru.CopyTo(stream);
                }

                if (!string.IsNullOrEmpty(guru.Foto))
                {
                    var pathFotoLama = Path.Combine(folder, guru.Foto);
                    if (System.IO.File.Exists(pathFotoLama))
                    {
                        System.IO.File.Delete(pathFotoLama);
                    }
                }

                guru.Foto = namaFile;
            }

            _context.SaveChanges();

            TempData["Sukses"] = "Profil berhasil diperbarui.";

            return RedirectToAction(nameof(ProfilSaya));
        }
    }
}
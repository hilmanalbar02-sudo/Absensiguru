using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;
using Absensiguru.ViewModels;

namespace Absensiguru.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // DASHBOARD ADMIN
        // ==========================
        public IActionResult Dashboard()
        {
            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            // 1. Total Guru Keseluruhan
            ViewBag.TotalGuru = _context.Gurus.Count();

            // 2. Guru Hadir Hari Ini
            ViewBag.Hadir = _context.Absensis
                .Count(a => a.Tanggal == hariIni && a.Status == "Hadir");

            // 3. Guru Izin / Sakit yang Disetujui Hari Ini
            ViewBag.Izin = _context.Izins
                .Count(i => i.Tanggal == hariIni && i.Status == "Disetujui");

            // 4. Guru Alpha Hari Ini (Total Guru - (Hadir + Izin))
            ViewBag.Alpha = ViewBag.TotalGuru - (ViewBag.Hadir + ViewBag.Izin);

            if (ViewBag.Alpha < 0)
            {
                ViewBag.Alpha = 0;
            }

            // 5. Query Menampilkan Guru yang Belum Absen & Belum Izin Hari Ini
            ViewBag.BelumAbsen = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .Where(g => 
                    !_context.Absensis.Any(a => a.IdGuru == g.IdGuru && a.Tanggal == hariIni) &&
                    !_context.Izins.Any(i => i.IdGuru == g.IdGuru && i.Tanggal == hariIni && i.Status == "Disetujui")
                )
                .OrderBy(g => g.NamaGuru)
                .ToList();

            return View();
        }

        // ==========================
        // ABSEN SAYA (ADMIN)
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

            var absensiHariIni = _context.Absensis
                .FirstOrDefault(a => a.IdGuru == idGuru && a.Tanggal == hariIni);

            ViewBag.AbsensiHariIni = absensiHariIni;

            return View(guru);
        }

        // ==========================
        // ABSEN MASUK ADMIN
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

            var cek = _context.Absensis.FirstOrDefault(a => a.IdGuru == idGuru && a.Tanggal == hariIni);

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
        // ABSEN PULANG ADMIN
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

            var absensi = _context.Absensis.FirstOrDefault(a => a.IdGuru == idGuru && a.Tanggal == hariIni);

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
        // RIWAYAT ABSENSI ADMIN
        // ==========================
        public IActionResult RiwayatSaya()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var riwayat = _context.Absensis
                .Where(a => a.IdGuru == idGuru.Value)
                .OrderByDescending(a => a.Tanggal)
                .ThenByDescending(a => a.JamMasuk)
                .ToList();

            return View(riwayat);
        }

        // ==========================
        // PROFIL ADMIN
        // ==========================
        [HttpGet]
        public IActionResult ProfilSaya()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var guru = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .FirstOrDefault(g => g.IdGuru == idGuru.Value);

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
    }
}
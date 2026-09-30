using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Absensiguru.Models;
using Absensiguru.ViewModels;

namespace Absensiguru.Controllers
{
    public class GuruController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GuruController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ==========================
        // DASHBOARD GURU
        // ==========================
        public IActionResult Dashboard()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var guru = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .FirstOrDefault(g => g.IdGuru == idGuru.Value);

            if (guru == null)
                return RedirectToAction("Index", "Login");

            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            var absensiHariIni = _context.Absensis
                .FirstOrDefault(a =>
                    a.IdGuru == idGuru.Value &&
                    a.Tanggal == hariIni);

            ViewBag.AbsensiHariIni = absensiHariIni as Absensi;

            // Mengambil 5 riwayat absensi terakhir untuk ditampilkan di dashboard
            ViewBag.Riwayat = _context.Absensis
                .Where(a => a.IdGuru == idGuru.Value)
                .OrderByDescending(a => a.Tanggal)
                .Take(5)
                .ToList();

            return View(guru);
        }
        
        // ==========================
        // RIWAYAT ABSENSI
        // ==========================
        public IActionResult Riwayat()
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
        // PENGAJUAN IZIN
        // ==========================
        [HttpGet]
        public IActionResult PengajuanIzin()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            return View(new IzinViewModel());
        }

        [HttpPost]
        public IActionResult PengajuanIzin(IzinViewModel model)
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
                return View(model);

            string? namaFile = null;

            // Upload bukti
            if (model.Bukti != null && model.Bukti.Length > 0)
            {
                string folder = Path.Combine(_environment.WebRootPath, "uploads", "izin");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                namaFile = Guid.NewGuid().ToString() + Path.GetExtension(model.Bukti.FileName);

                string path = Path.Combine(folder, namaFile);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    model.Bukti.CopyTo(stream);
                }
            }

            Izin izin = new Izin
            {
                IdGuru = idGuru.Value,
                Tanggal = model.Tanggal,
                JenisIzin = model.JenisIzin,
                Alasan = model.Alasan,
                Bukti = namaFile,
                Status = "Menunggu"
            };

            _context.Izins.Add(izin);
            _context.SaveChanges();

            TempData["Sukses"] = "Pengajuan izin berhasil dikirim.";

            return RedirectToAction(nameof(PengajuanIzin));
        }

        // ==========================
        // RIWAYAT IZIN
        // ==========================
        public IActionResult RiwayatIzin()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var data = _context.Izins
                .Where(i => i.IdGuru == idGuru.Value)
                .OrderByDescending(i => i.Tanggal)
                .ToList();

            return View(data);
        }

        // ==========================
        // PROFIL SAYA
        // ==========================
        [HttpGet]
        public IActionResult Profil()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var guru = _context.Gurus
                .Include(g => g.IdJabatanNavigation)
                .FirstOrDefault(g => g.IdGuru == idGuru.Value);

            if (guru == null)
                return RedirectToAction("Index", "Login");

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

        [HttpPost]
        public IActionResult Profil(ProfilGuruViewModel model)
        {
            var guru = _context.Gurus
                .FirstOrDefault(g => g.IdGuru == model.IdGuru);

            if (guru == null)
                return RedirectToAction(nameof(Profil));

            guru.Email = model.Email;
            guru.NoHp = model.NoHp;
            guru.Alamat = model.Alamat;

            // ==========================
            // UPLOAD FOTO
            // ==========================
            if (model.FotoBaru != null && model.FotoBaru.Length > 0)
            {
                string folder = Path.Combine(_environment.WebRootPath, "uploads", "guru");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Hapus foto lama jika ada
                if (!string.IsNullOrEmpty(guru.Foto))
                {
                    string oldFile = Path.Combine(folder, guru.Foto);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                string extension = Path.GetExtension(model.FotoBaru.FileName);
                string fileName = Guid.NewGuid().ToString() + extension;

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.FotoBaru.CopyTo(stream);
                }

                guru.Foto = fileName;
            }

            _context.SaveChanges();

            // Memperbarui session foto secara real-time setelah perubahan disimpan
            HttpContext.Session.SetString(
                "Foto",
                guru.Foto ?? ""
            );

            TempData["Sukses"] = "Profil berhasil diperbarui.";

            return RedirectToAction(nameof(Profil));
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

            // ==========================
            // VALIDASI JAM MASUK
            // ==========================
            TimeOnly mulaiMasuk = new TimeOnly(6, 30);
            TimeOnly batasMasuk = new TimeOnly(7, 30);
            TimeOnly sekarang = TimeOnly.FromDateTime(DateTime.Now);

            if (sekarang < mulaiMasuk)
            {
                TempData["Error"] = "Absensi masuk belum dibuka. Mulai pukul 06.30 WIB.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (sekarang > batasMasuk)
            {
                TempData["Error"] = "Waktu absensi masuk telah berakhir.";
                return RedirectToAction(nameof(Dashboard));
            }

            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            var absensi = _context.Absensis.FirstOrDefault(a =>
                a.IdGuru == idGuru.Value &&
                a.Tanggal == hariIni);

            if (absensi != null)
            {
                TempData["Error"] = "Anda sudah melakukan absensi hari ini.";
                return RedirectToAction(nameof(Dashboard));
            }

            var data = new Absensi
            {
                IdGuru = idGuru.Value,
                Tanggal = hariIni,
                JamMasuk = TimeOnly.FromDateTime(DateTime.Now),
                JamPulang = null,
                Status = "Hadir",
                Keterangan = "Absen Masuk"
            };

            _context.Absensis.Add(data);
            _context.SaveChanges();

            TempData["Sukses"] = "Absen masuk berhasil.";

            return RedirectToAction(nameof(Dashboard));
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

            // ==========================
            // VALIDASI JAM PULANG
            // ==========================
            TimeOnly mulaiPulang = new TimeOnly(13, 0);
            TimeOnly sekarang = TimeOnly.FromDateTime(DateTime.Now);

            if (sekarang < mulaiPulang)
            {
                TempData["Error"] = "Belum memasuki jam pulang. Absensi pulang dimulai pukul 13.00 WIB.";
                return RedirectToAction(nameof(Dashboard));
            }

            var hariIni = DateOnly.FromDateTime(DateTime.Today);

            var absensi = _context.Absensis.FirstOrDefault(a =>
                a.IdGuru == idGuru.Value &&
                a.Tanggal == hariIni);

            if (absensi == null)
            {
                TempData["Error"] = "Silakan lakukan absen masuk terlebih dahulu.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (absensi.JamPulang != null)
            {
                TempData["Error"] = "Anda sudah melakukan absen pulang.";
                return RedirectToAction(nameof(Dashboard));
            }

            absensi.JamPulang = TimeOnly.FromDateTime(DateTime.Now);

            _context.SaveChanges();

            TempData["Sukses"] = "Absen pulang berhasil.";

            return RedirectToAction(nameof(Dashboard));
        }

        // ==========================
        // UBAH PASSWORD
        // ==========================
        [HttpGet]
        public IActionResult UbahPassword()
        {
            var idGuru = HttpContext.Session.GetInt32("IdGuru");

            if (idGuru == null)
                return RedirectToAction("Index", "Login");

            var model = new UbahPasswordViewModel
            {
                IdGuru = idGuru.Value
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UbahPassword(UbahPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u => u.IdGuru == model.IdGuru);

            if (user == null)
            {
                TempData["Error"] = "Data user tidak ditemukan.";
                return View(model);
            }

            // Cek password lama
            if (user.Password != model.PasswordLama)
            {
                TempData["Error"] = "Password lama yang Anda masukkan salah.";
                return View(model);
            }

            // Cek konfirmasi password
            if (model.PasswordBaru != model.KonfirmasiPassword)
            {
                TempData["Error"] = "Konfirmasi password tidak sama.";
                return View(model);
            }

            // Simpan password baru
            user.Password = model.PasswordBaru;

            _context.SaveChanges();

            TempData["Sukses"] = "Password berhasil diubah.";

            return RedirectToAction(nameof(UbahPassword));
        }
    }
}
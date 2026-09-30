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

        // ==========================
        // LIST IZIN
        // ==========================
        public IActionResult Index()
        {
            var data = _context.Izins
                .Include(i => i.IdGuruNavigation)
                .OrderByDescending(i => i.Tanggal)
                .ToList();

            return View(data);
        }

        // ==========================
        // DETAIL IZIN
        // ==========================
        public IActionResult Detail(int id)
        {
            var izin = _context.Izins
                .Include(i => i.IdGuruNavigation)
                .ThenInclude(g => g.IdJabatanNavigation)
                .FirstOrDefault(i => i.IdIzin == id);

            if (izin == null)
                return RedirectToAction(nameof(Index));

            return View(izin);
        }

        // ==========================
        // SETUJUI IZIN
        // ==========================
        public IActionResult Setujui(int id)
        {
            var izin = _context.Izins.FirstOrDefault(i => i.IdIzin == id);

            if (izin == null)
                return RedirectToAction(nameof(Index));

            izin.Status = "Disetujui";

            // cek apakah absensi sudah ada
            var absen = _context.Absensis.FirstOrDefault(a =>
                a.IdGuru == izin.IdGuru &&
                a.Tanggal == izin.Tanggal);

            if (absen == null)
            {
                _context.Absensis.Add(new Absensi
                {
                    IdGuru = izin.IdGuru,
                    Tanggal = izin.Tanggal,
                    Status = izin.JenisIzin,
                    Keterangan = izin.Alasan
                });
            }

            _context.SaveChanges();

            TempData["Sukses"] = "Pengajuan izin berhasil disetujui.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // TOLAK IZIN
        // ==========================
        public IActionResult Tolak(int id)
        {
            var izin = _context.Izins.FirstOrDefault(i => i.IdIzin == id);

            if (izin == null)
                return RedirectToAction(nameof(Index));

            izin.Status = "Ditolak";

            _context.SaveChanges();

            TempData["Sukses"] = "Pengajuan izin berhasil ditolak.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // EDIT IZIN (GET)
        // ==========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var izin = _context.Izins
                .Include(i => i.IdGuruNavigation)
                .FirstOrDefault(i => i.IdIzin == id);

            if (izin == null)
                return RedirectToAction(nameof(Index));

            return View(izin);
        }

        // ==========================
        // SIMPAN EDIT (POST)
        // ==========================
        [HttpPost]
        public IActionResult Edit(Izin model)
        {
            var izin = _context.Izins.FirstOrDefault(i => i.IdIzin == model.IdIzin);

            if (izin == null)
                return RedirectToAction(nameof(Index));

            // Memperbarui field izin sesuai inputan form admin
            izin.JenisIzin = model.JenisIzin;
            izin.Tanggal = model.Tanggal;
            izin.Alasan = model.Alasan;

            _context.SaveChanges();

            TempData["Sukses"] = "Data izin berhasil diperbarui.";

            return RedirectToAction(nameof(Index));
        }
    }
}
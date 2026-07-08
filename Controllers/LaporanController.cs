using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Absensiguru.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Absensiguru.Controllers
{
    public class LaporanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaporanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateOnly? mulai, DateOnly? selesai, string? status)
        {
            var data = _context.Absensis
                .Include(x => x.IdGuruNavigation)
                .AsQueryable();

            if (mulai.HasValue)
                data = data.Where(x => x.Tanggal >= mulai.Value);

            if (selesai.HasValue)
                data = data.Where(x => x.Tanggal <= selesai.Value);

            if (!string.IsNullOrEmpty(status))
                data = data.Where(x => x.Status == status);

            ViewBag.Mulai = mulai;
            ViewBag.Selesai = selesai;
            ViewBag.Status = status;

            return View(await data
                .OrderByDescending(x => x.Tanggal)
                .ToListAsync());
        }

        // ===========================
        // EXPORT EXCEL
        // ===========================

        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.Absensis
                .Include(x => x.IdGuruNavigation)
                .OrderByDescending(x => x.Tanggal)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Laporan");

            ws.Cell(1, 1).Value = "No";
            ws.Cell(1, 2).Value = "Nama Guru";
            ws.Cell(1, 3).Value = "Tanggal";
            ws.Cell(1, 4).Value = "Jam Masuk";
            ws.Cell(1, 5).Value = "Jam Pulang";
            ws.Cell(1, 6).Value = "Status";

            int row = 2;
            int no = 1;

            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = no++;
                ws.Cell(row, 2).Value = item.IdGuruNavigation.NamaGuru;
                ws.Cell(row, 3).Value = item.Tanggal.ToString("dd/MM/yyyy");
                ws.Cell(row, 4).Value = item.JamMasuk?.ToString("HH:mm");
                ws.Cell(row, 5).Value = item.JamPulang?.ToString("HH:mm");
                ws.Cell(row, 6).Value = item.Status;

                row++;
            }

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "LaporanAbsensi.xlsx");
        }

        // ===========================
        // EXPORT PDF
        // ===========================

        public async Task<IActionResult> ExportPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var data = await _context.Absensis
                .Include(x => x.IdGuruNavigation)
                .OrderByDescending(x => x.Tanggal)
                .ToListAsync();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);

                    page.Header()
                        .Text("LAPORAN ABSENSI GURU")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("No").Bold();
                            header.Cell().Text("Guru").Bold();
                            header.Cell().Text("Tanggal").Bold();
                            header.Cell().Text("Masuk").Bold();
                            header.Cell().Text("Pulang").Bold();
                            header.Cell().Text("Status").Bold();
                        });

                        int no = 1;

                        foreach (var item in data)
                        {
                            table.Cell().Text(no++.ToString());
                            table.Cell().Text(item.IdGuruNavigation.NamaGuru);
                            table.Cell().Text(item.Tanggal.ToString("dd/MM/yyyy"));
                            table.Cell().Text(item.JamMasuk?.ToString("HH:mm") ?? "-");
                            table.Cell().Text(item.JamPulang?.ToString("HH:mm") ?? "-");
                            table.Cell().Text(item.Status);
                        }
                    });
                });
            });

            return File(
                pdf.GeneratePdf(),
                "application/pdf",
                "LaporanAbsensi.pdf");
        }
    }
}
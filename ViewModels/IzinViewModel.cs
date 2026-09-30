using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Absensiguru.ViewModels
{
    public class IzinViewModel
    {
        public DateOnly Tanggal { get; set; }

        [Required]
        public string JenisIzin { get; set; } = "";

        [Required]
        public string Alasan { get; set; } = "";

        public IFormFile? Bukti { get; set; }
    }
}
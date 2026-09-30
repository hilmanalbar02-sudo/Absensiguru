using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Absensiguru.ViewModels
{
    public class ProfilGuruViewModel
    {
        public int IdGuru { get; set; }

        public string Nip { get; set; } = "";

        public string NamaGuru { get; set; } = "";

        public string Jabatan { get; set; } = "";

        public string? Email { get; set; }

        public string? NoHp { get; set; }

        public string? Alamat { get; set; }

        public string? Foto { get; set; }

        [Display(Name = "Foto Profil")]
        public IFormFile? FotoBaru { get; set; }
    }
}
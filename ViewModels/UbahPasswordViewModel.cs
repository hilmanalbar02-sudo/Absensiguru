using System.ComponentModel.DataAnnotations;

namespace Absensiguru.ViewModels
{
    public class UbahPasswordViewModel
    {
        public int IdGuru { get; set; }

        [Required]
        [Display(Name = "Password Lama")]
        public string PasswordLama { get; set; } = "";

        [Required]
        [Display(Name = "Password Baru")]
        public string PasswordBaru { get; set; } = "";

        [Required]
        [Compare("PasswordBaru", ErrorMessage = "Konfirmasi password tidak sama.")]
        [Display(Name = "Konfirmasi Password")]
        public string KonfirmasiPassword { get; set; } = "";
    }
}
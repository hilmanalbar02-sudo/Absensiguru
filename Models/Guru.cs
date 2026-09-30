using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Absensiguru.Models;

public partial class Guru
{
    public int IdGuru { get; set; }

    public string Nip { get; set; } = null!;

    public string NamaGuru { get; set; } = null!;

    public string JenisKelamin { get; set; } = null!;

    public string? Alamat { get; set; }

    public string? NoHp { get; set; }

    public string? Email { get; set; }

    // Kolom baru untuk menyimpan nama file foto
    public string? Foto { get; set; }

    public string? Status { get; set; }

    public int IdJabatan { get; set; }

    [ValidateNever]
    public virtual ICollection<Absensi> Absensis { get; set; } = new List<Absensi>();

    [ValidateNever]
    public virtual Jabatan IdJabatanNavigation { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<Izin> Izins { get; set; } = new List<Izin>();

    [ValidateNever]
    public virtual User? User { get; set; }
}
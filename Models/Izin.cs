using System;
using System.Collections.Generic;

namespace Absensiguru.Models;

public partial class Izin
{
    public int IdIzin { get; set; }

    public int IdGuru { get; set; }

    public DateOnly Tanggal { get; set; }

    public string JenisIzin { get; set; } = null!;

    public string Alasan { get; set; } = null!;

    public string? Bukti { get; set; }

    public string? Status { get; set; }

    public virtual Guru IdGuruNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Absensiguru.Models;

public partial class Absensi
{
    public int IdAbsensi { get; set; }

    public int IdGuru { get; set; }

    public DateOnly Tanggal { get; set; }

    public TimeOnly? JamMasuk { get; set; }

    public TimeOnly? JamPulang { get; set; }

    public string Status { get; set; } = null!;

    public string? Keterangan { get; set; }

    public virtual Guru IdGuruNavigation { get; set; } = null!;
}

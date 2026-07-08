using System;
using System.Collections.Generic;

namespace Absensiguru.Models;

public partial class Jabatan
{
    public int IdJabatan { get; set; }

    public string NamaJabatan { get; set; } = null!;

    public virtual ICollection<Guru> Gurus { get; set; } = new List<Guru>();
}

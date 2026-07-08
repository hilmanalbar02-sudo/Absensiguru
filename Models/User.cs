using System;
using System.Collections.Generic;

namespace Absensiguru.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? IdGuru { get; set; }

    public virtual Guru? IdGuruNavigation { get; set; }
}

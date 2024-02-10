using System;
using System.Collections.Generic;

namespace LangaraCPSC.WebAPI.DbModels;

public partial class Exec
{
    public int Id { get; set; }

    public string Firstname { get; set; } = null!;

    public string? Lastname { get; set; }

    public string Email { get; set; } = null!;

    public int Position { get; set; }

    public string Tenurestart { get; set; } = null!;

    public string? Tenureend { get; set; }
}

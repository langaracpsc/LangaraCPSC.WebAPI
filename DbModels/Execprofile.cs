using System;
using System.Collections.Generic;

namespace LangaraCPSC.WebAPI.DbModels;

public partial class Execprofile
{
    public int Id { get; set; }

    public string Imageid { get; set; } = null!;

    public string Description { get; set; } = null!;
}

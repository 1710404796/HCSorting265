using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCSortingCZP.Data.Models;

public class NgBeltCellLog
{
    public int Id { get; set; }

    public DateTime RecordTime { get; set; } = DateTime.Now;

    [Column(TypeName = "nvarchar(64)")]
    public string Key { get; set; } = string.Empty;

    public int FromInsideNum { get; set; }

    [Column(TypeName = "nvarchar(64)")]
    public string FromStationName { get; set; } = string.Empty;

    public int ToInsideNum { get; set; }

    [Column(TypeName = "nvarchar(64)")]
    public string ToStationName { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(64)")]
    public string CellCode { get; set; } = string.Empty;

    public int FromChannel { get; set; }

    public int ToChannel { get; set; }
}

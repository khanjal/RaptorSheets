using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class RoomEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Room, isInput: true)]
    public string Room { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.RoomLength, isInput: true, formatType: FormatEnum.NUMBER)]
    public decimal? Length { get; set; }

    [Column(SheetsConfig.HeaderNames.RoomWidth, isInput: true, formatType: FormatEnum.NUMBER)]
    public decimal? Width { get; set; }

    // Calculated: L x W (configured in RoomMapper)
    [Column(SheetsConfig.HeaderNames.SquareFeet, FormatEnum.NUMBER, ColumnNotes.SquareFeet)]
    public decimal? SquareFeet { get; set; }

    [Column(SheetsConfig.HeaderNames.Level, isInput: true)]
    public string Level { get; set; } = "";
}

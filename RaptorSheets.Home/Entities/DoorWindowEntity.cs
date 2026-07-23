using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

/// <summary>
/// Covers both doors and windows - they share the same shape (a dimensioned opening/fixture with a
/// brand, model, and install date), so one sheet holds both. <see cref="Type"/> is free text (e.g.
/// "Front Door", "Bay Window", "Sliding Door") and is what distinguishes the two.
/// </summary>
[ExcludeFromCodeCoverage]
public class DoorWindowEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Location, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRoom)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Type, isInput: true)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Color, isInput: true)]
    public string Color { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Width, isInput: true)]
    public string Width { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Height, isInput: true)]
    public string Height { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Depth, isInput: true)]
    public string Depth { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Hinge, isInput: true)]
    public string Hinge { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Brand, isInput: true)]
    public string Brand { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Model, isInput: true)]
    public string Model { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.SerialNumber, isInput: true)]
    public string SerialNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Installed, isInput: true, note: ColumnNotes.DateFormat, formatType: Format.DATE)]
    public string Installed { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";
}

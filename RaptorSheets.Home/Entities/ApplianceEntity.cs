using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class ApplianceEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Type, isInput: true)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Location, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRoom)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Manufacturer, isInput: true)]
    public string Manufacturer { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Model, isInput: true)]
    public string Model { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.SerialNumber, isInput: true)]
    public string SerialNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ManufactureDate, isInput: true, note: ColumnNotes.DateFormat, formatType: Format.DATE)]
    public string ManufactureDate { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.EnergySource, isInput: true)]
    public string EnergySource { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AverageUsage, isInput: true)]
    public string AverageUsage { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Capacity, isInput: true)]
    public string Capacity { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Filter, isInput: true)]
    public string Filter { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.FilterDate, isInput: true, note: ColumnNotes.DateFormat, formatType: Format.DATE)]
    public string FilterDate { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ReplacementMonths, isInput: true, note: ColumnNotes.ReplacementMonths, formatType: Format.NUMBER)]
    public int? ReplacementMonths { get; set; }

    // Calculated: Filter Date + Rpl. Mth (configured in ApplianceMapper)
    [Column(SheetsConfig.HeaderNames.NextFilter, Format.DATE, ColumnNotes.NextFilter)]
    public string NextFilter { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Other, isInput: true)]
    public string Other { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.OriginalPrice, isInput: true, formatType: Format.ACCOUNTING)]
    public decimal? OriginalPrice { get; set; }
}

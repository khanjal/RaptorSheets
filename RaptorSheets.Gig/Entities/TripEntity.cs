using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class TripEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, FormatEnum.DATE, isInput: true, note: ColumnNotes.DateFormat)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true, jsonPropertyName: "number", note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Exclude, isInput: true, jsonPropertyName: "exclude", note: ColumnNotes.Exclude, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool Exclude { get; set; } = false;

    [Column(SheetsConfig.HeaderNames.Type, isInput: true, note: ColumnNotes.Types, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeType)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Place, isInput: true, note: ColumnNotes.Place, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePlace)]
    public string Place { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pickup, FormatEnum.TIME, isInput: true, jsonPropertyName: "pickupTime", note: ColumnNotes.Pickup)]
    public string Pickup { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Dropoff, FormatEnum.TIME, isInput: true, jsonPropertyName: "dropoffTime")]
    public string Dropoff { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, FormatEnum.DURATION, isInput: true, note: ColumnNotes.Duration)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pay, isInput: true)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, isInput: true)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, isInput: true)]
    public decimal? Bonus { get; set; }

    // Output column (formula: Pay + Tips + Bonus) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Total)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, isInput: true)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart, ColumnOptions.Builder()
        .AsInput()
        .WithFormatPattern(CellFormatPatterns.Distance)
        .WithJsonPropertyName("startOdometer"))]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd, ColumnOptions.Builder()
        .AsInput()
        .WithFormatPattern(CellFormatPatterns.Distance)
        .WithJsonPropertyName("endOdometer"))]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, ColumnOptions.Builder()
        .AsInput()
        .WithFormatPattern(CellFormatPatterns.Distance)
        .WithJsonPropertyName("distance")
        .WithNote(ColumnNotes.TripDistance))]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Name, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeName)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressStart, isInput: true, jsonPropertyName: "startAddress", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string StartAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressEnd, isInput: true, jsonPropertyName: "endAddress", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string EndAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.UnitEnd, isInput: true, jsonPropertyName: "endUnit", note: ColumnNotes.UnitTypes)]
    public string EndUnit { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.OrderNumber, isInput: true, jsonPropertyName: "orderNumber")]
    public string OrderNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Region, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, isInput: true)]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - default to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, note: ColumnNotes.TripKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, jsonPropertyName: "amountPerTime")]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, jsonPropertyName: "amountPerDistance")]
    public decimal? AmountPerDistance { get; set; }
}
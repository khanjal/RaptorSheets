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
    [Column(SheetsConfig.HeaderNames.Date, isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
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

    [Column(SheetsConfig.HeaderNames.Pickup, isInput: true, jsonPropertyName: "pickupTime", note: ColumnNotes.Pickup, formatType: FormatEnum.TIME)]
    public string Pickup { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Dropoff, isInput: true, jsonPropertyName: "dropoffTime", formatType: FormatEnum.TIME)]
    public string Dropoff { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, isInput: true, note: ColumnNotes.Duration, formatType: FormatEnum.DURATION)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pay, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    // Output column (formula: Pay + Tips + Bonus) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Total, FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart,
        isInput: true,
        jsonPropertyName: "startOdometer",
        formatPattern: CellFormatPatterns.Distance)]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd,
        isInput: true,
        jsonPropertyName: "endOdometer",
        formatPattern: CellFormatPatterns.Distance)]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance,
        isInput: true,
        jsonPropertyName: "distance",
        formatPattern: CellFormatPatterns.Distance,
        note: ColumnNotes.TripDistance)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Name, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeName)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressStart, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string StartAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressEnd, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string EndAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.UnitEnd, isInput: true, note: ColumnNotes.UnitTypes)]
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

    [Column(SheetsConfig.HeaderNames.AmountPerTime, jsonPropertyName: "amountPerTime", formatType: FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, jsonPropertyName: "amountPerDistance", formatType: FormatEnum.ACCOUNTING)]
    public decimal? AmountPerDistance { get; set; }
}
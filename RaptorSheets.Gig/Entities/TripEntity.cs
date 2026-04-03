using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class TripEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Header(SheetsConfig.HeaderNames.Date)]
    [Input]
    [Format(FormatEnum.DATE)]
    public string Date { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Service)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Number)]
    [Input]
    [Note(ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Header(SheetsConfig.HeaderNames.Exclude)]
    [Input]
    [Note(ColumnNotes.Exclude)]
    [Validation(SheetsConfig.ValidationNames.Boolean)]
    public bool Exclude { get; set; } = false;

    [Header(SheetsConfig.HeaderNames.Type)]
    [Input]
    [Note(ColumnNotes.Types)]
    [Validation(SheetsConfig.ValidationNames.RangeType)]
    public string Type { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Place)]
    [Input]
    [Note(ColumnNotes.Place)]
    [Validation(SheetsConfig.ValidationNames.RangePlace)]
    public string Place { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Pickup)]
    [Input]
    [Note(ColumnNotes.Pickup)]
    [Format(FormatEnum.TIME)]
    [JsonPropertyName("pickupTime")]
    public string Pickup { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Dropoff)]
    [Input]
    [Format(FormatEnum.TIME)]
    [JsonPropertyName("dropoffTime")]
    public string Dropoff { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Duration)]
    [Input]
    [Note(ColumnNotes.Duration)]
    [Format(FormatEnum.DURATION)]
    public string Duration { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Pay)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Header(SheetsConfig.HeaderNames.Tips)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Header(SheetsConfig.HeaderNames.Bonus)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    // Output column (formula: Pay + Tips + Bonus) - defaults to isInput: false
    [Header(SheetsConfig.HeaderNames.Total)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Header(SheetsConfig.HeaderNames.Cash)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Header(SheetsConfig.HeaderNames.OdometerStart)]
    [Input]
    [Format(FormatEnum.DISTANCE)]
    [JsonPropertyName("startOdometer")]
    public decimal? OdometerStart { get; set; }

    [Header(SheetsConfig.HeaderNames.OdometerEnd)]
    [Input]
    [Format(FormatEnum.DISTANCE)]
    [JsonPropertyName("endOdometer")]
    public decimal? OdometerEnd { get; set; }

    [Header(SheetsConfig.HeaderNames.Distance)]
    [Input]
    [Format(FormatEnum.DISTANCE)]
    [Note(ColumnNotes.TripDistance)]
    public decimal? Distance { get; set; }

    [Header(SheetsConfig.HeaderNames.Name)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeName)]
    public string Name { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.AddressStart)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeAddress)]
    public string StartAddress { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.AddressEnd)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeAddress)]
    public string EndAddress { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.UnitEnd)]
    [Input]
    [Note(ColumnNotes.UnitTypes)]
    public string EndUnit { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.OrderNumber)]
    [Input]
    public string OrderNumber { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Region)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Note)]
    [Input]
    public string Note { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Tags)]
    [Input]
    public string Tags { get; set; } = string.Empty;

    // Output columns (formulas/calculated) - default to isInput: false
    [Header(SheetsConfig.HeaderNames.Key)]
    [Note(ColumnNotes.TripKey)]
    public string Key { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.AmountPerTime)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTime { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDistance)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? AmountPerDistance { get; set; }
}
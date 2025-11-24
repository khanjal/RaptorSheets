using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ShiftEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, isInput: true, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeStart, isInput: true, formatType: FormatEnum.TIME)]
    public string Start { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeEnd, isInput: true, formatType: FormatEnum.TIME)]
    public string Finish { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true, note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeActive, isInput: true, note: ColumnNotes.ActiveTime, formatType: FormatEnum.DURATION)]
    public string Active { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeTotal, isInput: true, note: ColumnNotes.TotalTime, formatType: FormatEnum.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeOmit, isInput: true, note: ColumnNotes.TimeOmit, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool? Omit { get; set; }

    [Column(SheetsConfig.HeaderNames.Trips, isInput: true, note: ColumnNotes.ShiftTrips)]
    public int Trips { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart,
        isInput: true,
        formatPattern: CellFormatPatterns.Distance)]
    [JsonPropertyName("startOdometer")]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd,
        isInput: true,
        formatPattern: CellFormatPatterns.Distance)]
    [JsonPropertyName("endOdometer")]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance,
        isInput: true,
        formatPattern: CellFormatPatterns.Distance,
        note: ColumnNotes.ShiftDistance)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Region, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, isInput: true)]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, isInput: false, note: ColumnNotes.ShiftKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTimeActive, FormatEnum.DURATION, ColumnNotes.TotalTimeActive)]
    public string TotalActive { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTime, FormatEnum.DURATION)]
    public string TotalTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTrips, note: ColumnNotes.TotalTrips)]
    public int TotalTrips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalPay, FormatEnum.ACCOUNTING)]
    public decimal? TotalPay { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalTips, FormatEnum.ACCOUNTING)]
    public decimal? TotalTips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalBonus, FormatEnum.ACCOUNTING)]
    public decimal? TotalBonus { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalGrand, FormatEnum.ACCOUNTING)]
    public decimal? GrandTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalCash, FormatEnum.ACCOUNTING)]
    public decimal? TotalCash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalDistance, FormatEnum.DISTANCE, ColumnNotes.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FormatEnum.ACCOUNTING)]
    public decimal? AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TripsPerHour, FormatEnum.DISTANCE)]
    public decimal? TripsPerHour { get; set; }

    [Column(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";
}

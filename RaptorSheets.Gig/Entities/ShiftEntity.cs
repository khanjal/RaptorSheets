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
        [Header(SheetsConfig.HeaderNames.Date)]
        [Input]
        [Format(FormatEnum.DATE)]
        public string Date { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TimeStart)]
    [Input]
    [Format(FormatEnum.TIME)]
    public string Start { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TimeEnd)]
    [Input]
    [Format(FormatEnum.TIME)]
    public string Finish { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Service)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Number)]
    [Input]
    [Note(ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Header(SheetsConfig.HeaderNames.TimeActive)]
    [Input]
    [Note(ColumnNotes.ActiveTime)]
    [Format(FormatEnum.DURATION)]
    public string Active { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TimeTotal)]
    [Input]
    [Note(ColumnNotes.TotalTime)]
    [Format(FormatEnum.DURATION)]
    public string Time { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TimeOmit)]
    [Input]
    [Note(ColumnNotes.TimeOmit)]
    [Validation(SheetsConfig.ValidationNames.Boolean)]
    public bool? Omit { get; set; }

    [Header(SheetsConfig.HeaderNames.Trips)]
    [Input]
    [Note(ColumnNotes.ShiftTrips)]
    public int? Trips { get; set; }

    // Financial properties
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

    [Header(SheetsConfig.HeaderNames.Cash)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Header(SheetsConfig.HeaderNames.OdometerStart)]
    [Input]
    [Format(CellFormatPatterns.Distance)]
    [JsonPropertyName("startOdometer")]
    public decimal? OdometerStart { get; set; }

    [Header(SheetsConfig.HeaderNames.OdometerEnd)]
    [Input]
    [Format(CellFormatPatterns.Distance)]
    [JsonPropertyName("endOdometer")]
    public decimal? OdometerEnd { get; set; }

    [Header(SheetsConfig.HeaderNames.Distance)]
    [Input]
    [Format(CellFormatPatterns.Distance)]
    [Note(ColumnNotes.ShiftDistance)]
    public decimal? Distance { get; set; }

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

    // Output columns (formulas/calculated) - defaults to isInput: false
    [Header(SheetsConfig.HeaderNames.Key)]
    [Note(ColumnNotes.ShiftKey)]
    public string Key { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TotalTimeActive)]
    [Format(FormatEnum.DURATION)]
    [Note(ColumnNotes.TotalTimeActive)]
    public string TotalActive { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TotalTime)]
    [Format(FormatEnum.DURATION)]
    public string TotalTime { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.TotalTrips)]
    [Note(ColumnNotes.TotalTrips)]
    public int TotalTrips { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalPay)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? TotalPay { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalTips)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? TotalTips { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalBonus)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? TotalBonus { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalGrand)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? GrandTotal { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalCash)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? TotalCash { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerTrip)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTrip { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerTime)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? AmountPerTime { get; set; }

    [Header(SheetsConfig.HeaderNames.TotalDistance)]
    [Format(FormatEnum.DISTANCE)]
    [Note(ColumnNotes.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDistance)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? AmountPerDistance { get; set; }

    [Header(SheetsConfig.HeaderNames.TripsPerHour)]
    [Format(FormatEnum.DISTANCE)]
    public decimal? TripsPerHour { get; set; }

    [Header(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";
}

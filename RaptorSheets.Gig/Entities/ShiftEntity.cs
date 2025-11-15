using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ShiftEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, isInput: true, jsonPropertyName: "date", note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeStart, isInput: true, jsonPropertyName: "start", formatType: FormatEnum.TIME)]
    public string Start { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeEnd, isInput: true, jsonPropertyName: "finish", formatType: FormatEnum.TIME)]
    public string Finish { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, isInput: true, jsonPropertyName: "service", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true, jsonPropertyName: "number", note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeActive, isInput: true, jsonPropertyName: "active", note: ColumnNotes.ActiveTime, formatType: FormatEnum.DURATION)]
    public string Active { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeTotal, isInput: true, jsonPropertyName: "time", note: ColumnNotes.TotalTime, formatType: FormatEnum.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeOmit, isInput: true, jsonPropertyName: "omit", note: ColumnNotes.TimeOmit, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool? Omit { get; set; }

    [Column(SheetsConfig.HeaderNames.Trips, isInput: true, jsonPropertyName: "trips", note: ColumnNotes.ShiftTrips)]
    public int Trips { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, isInput: true, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, isInput: true, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, isInput: true, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, isInput: true, jsonPropertyName: "cash")]
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
        note: ColumnNotes.ShiftDistance)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Region, isInput: true, jsonPropertyName: "region", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, isInput: true, jsonPropertyName: "note")]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, jsonPropertyName: "key", note: ColumnNotes.ShiftKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTimeActive, jsonPropertyName: "totalActive", note: ColumnNotes.TotalTimeActive, formatType: FormatEnum.DURATION)]
    public string TotalActive { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTime, jsonPropertyName: "totalTime", formatType: FormatEnum.DURATION)]
    public string TotalTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTrips, jsonPropertyName: "totalTrips", note: ColumnNotes.TotalTrips)]
    public int TotalTrips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalPay, jsonPropertyName: "totalPay")]
    public decimal? TotalPay { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalTips, jsonPropertyName: "totalTips")]
    public decimal? TotalTips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalBonus, jsonPropertyName: "totalBonus")]
    public decimal? TotalBonus { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalGrand, jsonPropertyName: "grandTotal")]
    public decimal? GrandTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalCash, jsonPropertyName: "totalCash")]
    public decimal? TotalCash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, jsonPropertyName: "amountPerTrip")]
    public decimal? AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTime, jsonPropertyName: "amountPerTime")]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalDistance,
        jsonPropertyName: "totalDistance",
        note: ColumnNotes.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, jsonPropertyName: "amountPerDistance")]
    public decimal? AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TripsPerHour)]
    public decimal? TripsPerHour { get; set; }

    [Column(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";
}

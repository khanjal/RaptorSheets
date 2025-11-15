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
    [Column(SheetsConfig.HeaderNames.Date, FormatEnum.DATE, isInput: true, jsonPropertyName: "date", note: ColumnNotes.DateFormat)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeStart, FormatEnum.TIME, isInput: true, jsonPropertyName: "start")]
    public string Start { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeEnd, FormatEnum.TIME, isInput: true, jsonPropertyName: "finish")]
    public string Finish { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, isInput: true, jsonPropertyName: "service", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true, jsonPropertyName: "number", note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeActive, FormatEnum.DURATION, isInput: true, jsonPropertyName: "active", note: ColumnNotes.ActiveTime)]
    public string Active { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeTotal, FormatEnum.DURATION, isInput: true, jsonPropertyName: "time", note: ColumnNotes.TotalTime)]
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
        .WithNote(ColumnNotes.ShiftDistance))]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Region, isInput: true, jsonPropertyName: "region", enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, isInput: true, jsonPropertyName: "note")]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, jsonPropertyName: "key", note: ColumnNotes.ShiftKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTimeActive, FormatEnum.DURATION, jsonPropertyName: "totalActive", note: ColumnNotes.TotalTimeActive)]
    public string TotalActive { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTime, FormatEnum.DURATION, jsonPropertyName: "totalTime")]
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

    [Column(SheetsConfig.HeaderNames.TotalDistance, ColumnOptions.Builder()
        .WithJsonPropertyName("totalDistance")
        .WithNote(ColumnNotes.TotalDistance))]
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

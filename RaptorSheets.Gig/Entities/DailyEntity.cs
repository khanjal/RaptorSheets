using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class DailyEntity : SheetRowEntityBase
{
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, FormatEnum.DATE, jsonPropertyName: "date")]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Pay, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, jsonPropertyName: "total")]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, jsonPropertyName: "cash")]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, jsonPropertyName: "amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, jsonPropertyName: "amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, FormatEnum.DURATION, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, jsonPropertyName: "amt/hour")]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.Day, jsonPropertyName: "day")]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Week, jsonPropertyName: "week")]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month, jsonPropertyName: "month")]
    public string Month { get; set; } = "";
}

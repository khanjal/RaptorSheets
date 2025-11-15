using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class DailyEntity : SheetRowEntityBase
{
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, formatType: FormatEnum.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.Day    )]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Week)]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";
}

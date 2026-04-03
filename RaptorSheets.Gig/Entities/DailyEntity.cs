using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class DailyEntity : SheetRowEntityBase
{
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
        [Header(SheetsConfig.HeaderNames.Date)]
        [Format(FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Header(SheetsConfig.HeaderNames.Pay)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Header(SheetsConfig.HeaderNames.Tips)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Header(SheetsConfig.HeaderNames.Bonus)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Header(SheetsConfig.HeaderNames.Total)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Header(SheetsConfig.HeaderNames.Cash)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerTrip)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Header(SheetsConfig.HeaderNames.Distance)]
    [Format(FormatEnum.DISTANCE)]
    public decimal Distance { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDistance)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    [Header(SheetsConfig.HeaderNames.TimeTotal)]
    [Format(FormatEnum.DURATION)]
    public string Time { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.AmountPerTime)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Header(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Week)]
    public string Week { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";
}

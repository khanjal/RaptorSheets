using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class MonthlyEntity : SheetRowEntityBase
{
    [Header(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Header(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties
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
    public string TimeTotal { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.AmountPerTime)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDay)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerDay { get; set; }

    [Header(SheetsConfig.HeaderNames.Average)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal Average { get; set; }

    [Header(SheetsConfig.HeaderNames.Number)]
    public int Number { get; set; }

    [Header(SheetsConfig.HeaderNames.Year)]
    public int Year { get; set; }
}

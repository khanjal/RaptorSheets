using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeeklyEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Week)]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, Format.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, Format.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, Format.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, Format.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, Format.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, Format.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, Format.DISTANCE)]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, Format.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, Format.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, Format.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay, Format.ACCOUNTING)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.Average, Format.ACCOUNTING)]
    public decimal Average { get; set; }

    [Column(SheetsConfig.HeaderNames.Number)]
    public int Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Year)]
    public int Year { get; set; }

    [Column(SheetsConfig.HeaderNames.DateBegin, Format.DATE)]
    public string Begin { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.DateEnd, Format.DATE)]
    public string End { get; set; } = "";
}

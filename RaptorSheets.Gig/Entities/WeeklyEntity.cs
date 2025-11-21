using RaptorSheets.Core.Attributes;
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
    [Column(SheetsConfig.HeaderNames.Pay, FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FormatEnum.ACCOUNTING, "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FormatEnum.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FormatEnum.DISTANCE, "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FormatEnum.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, FormatEnum.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FormatEnum.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay, FormatEnum.ACCOUNTING)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.Average, FormatEnum.ACCOUNTING)]
    public decimal Average { get; set; }

    [Column(SheetsConfig.HeaderNames.Number)]
    public int Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Year)]
    public int Year { get; set; }

    [Column(SheetsConfig.HeaderNames.DateBegin, FormatEnum.DATE)]
    public string Begin { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.DateEnd, FormatEnum.DATE)]
    public string End { get; set; } = "";
}

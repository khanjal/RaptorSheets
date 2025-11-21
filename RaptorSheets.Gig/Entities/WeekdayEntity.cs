using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeekdayEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Day)]
    public int Day { get; set; }

    [Column(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

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

    [Column(SheetsConfig.HeaderNames.AmountPerDay, FormatEnum.ACCOUNTING, "dailyAverage")]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountCurrent, FormatEnum.ACCOUNTING, "currentAmount")]
    public decimal CurrentAmount { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPrevious, FormatEnum.ACCOUNTING, "previousAmount")]
    public decimal PreviousAmount { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerPreviousDay, FormatEnum.ACCOUNTING, "dailyPrevAverage")]
    public decimal AmountPerPrevDay { get; set; }
}

using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ServiceEntity : SheetRowEntityBase
{
    // Entity-specific property first
    [Header(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    // CommonTripSheetHeaders pattern
    [Header(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // CommonIncomeHeaders
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

    // Visit properties
    [Header(SheetsConfig.HeaderNames.VisitFirst)]
    [JsonPropertyName("firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.VisitLast)]
    [JsonPropertyName("lastTrip")]
    public string LastTrip { get; set; } = "";
}

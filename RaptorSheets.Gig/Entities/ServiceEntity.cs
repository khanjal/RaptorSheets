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
    [Column(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    // CommonTripSheetHeaders pattern
    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // CommonIncomeHeaders
    [Column(SheetsConfig.HeaderNames.Pay, FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FormatEnum.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FormatEnum.DISTANCE)]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FormatEnum.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    // Visit properties
    [Column(SheetsConfig.HeaderNames.VisitFirst)]
    [JsonPropertyName("firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast)]
    [JsonPropertyName("lastTrip")]
    public string LastTrip { get; set; } = "";
}

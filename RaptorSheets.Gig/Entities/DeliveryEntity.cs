using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class DeliveryEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Address)]
    public string Address { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Pay, Format.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, Format.ACCOUNTING)]
    public decimal? Tips { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, Format.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, Format.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, Format.DISTANCE)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.VisitFirst)]
    [JsonPropertyName("firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast)]
    [JsonPropertyName("lastTrip")]
    public string LastTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, Format.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, Format.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }
}

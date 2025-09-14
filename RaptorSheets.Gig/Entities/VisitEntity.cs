using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class VisitEntity : AmountEntity
{
    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [JsonPropertyName("first trip")]
    [SheetOrder(SheetsConfig.HeaderNames.VisitFirst)]
    public string FirstTrip { get; set; } = "";

    [JsonPropertyName("last trip")]
    [SheetOrder(SheetsConfig.HeaderNames.VisitLast)]
    public string LastTrip { get; set; } = "";
}
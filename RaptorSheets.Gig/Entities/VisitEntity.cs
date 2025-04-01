using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class VisitEntity : AmountEntity
{
    [JsonPropertyName("trips")]
    public int Trips { get; set; }

    [JsonPropertyName("first trip")]
    public string FirstTrip { get; set; } = "";

    [JsonPropertyName("last trip")]
    public string LastTrip { get; set; } = "";
}
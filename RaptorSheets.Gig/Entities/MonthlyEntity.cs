using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class MonthlyEntity 
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [Column(SheetsConfig.HeaderNames.Month, jsonPropertyName: "month")]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Days, jsonPropertyName: "days")]
    public int Days { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, jsonPropertyName: "total")]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, jsonPropertyName: "cash")]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, jsonPropertyName: "amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, jsonPropertyName: "amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, FormatEnum.DURATION, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, jsonPropertyName: "amt/hour")]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.Average, jsonPropertyName: "average")]
    public decimal Average { get; set; }

    [Column(SheetsConfig.HeaderNames.Number, jsonPropertyName: "#")]
    public int Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Year, jsonPropertyName: "year")]
    public int Year { get; set; }
}

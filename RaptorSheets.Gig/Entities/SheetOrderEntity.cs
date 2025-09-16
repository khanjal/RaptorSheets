using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Entity that defines the sheet ordering for the Gig workbook.
/// Uses SheetOrder attributes to specify the tab order in the spreadsheet.
/// </summary>
public class SheetOrderEntity
{
    [JsonPropertyName("trips")]
    [SheetOrder(0, SheetsConfig.SheetNames.Trips)]
    public bool Trips { get; set; } = true;

    [JsonPropertyName("shifts")]
    [SheetOrder(1, SheetsConfig.SheetNames.Shifts)]
    public bool Shifts { get; set; } = true;

    [JsonPropertyName("expenses")]
    [SheetOrder(2, SheetsConfig.SheetNames.Expenses)]
    public bool Expenses { get; set; } = true;

    [JsonPropertyName("addresses")]
    [SheetOrder(3, SheetsConfig.SheetNames.Addresses)]
    public bool Addresses { get; set; } = true;

    [JsonPropertyName("names")]
    [SheetOrder(4, SheetsConfig.SheetNames.Names)]
    public bool Names { get; set; } = true;

    [JsonPropertyName("places")]
    [SheetOrder(5, SheetsConfig.SheetNames.Places)]
    public bool Places { get; set; } = true;

    [JsonPropertyName("regions")]
    [SheetOrder(6, SheetsConfig.SheetNames.Regions)]
    public bool Regions { get; set; } = true;

    [JsonPropertyName("services")]
    [SheetOrder(7, SheetsConfig.SheetNames.Services)]
    public bool Services { get; set; } = true;

    [JsonPropertyName("types")]
    [SheetOrder(8, SheetsConfig.SheetNames.Types)]
    public bool Types { get; set; } = true;

    [JsonPropertyName("daily")]
    [SheetOrder(9, SheetsConfig.SheetNames.Daily)]
    public bool Daily { get; set; } = true;

    [JsonPropertyName("weekdays")]
    [SheetOrder(10, SheetsConfig.SheetNames.Weekdays)]
    public bool Weekdays { get; set; } = true;

    [JsonPropertyName("weekly")]
    [SheetOrder(11, SheetsConfig.SheetNames.Weekly)]
    public bool Weekly { get; set; } = true;

    [JsonPropertyName("monthly")]
    [SheetOrder(12, SheetsConfig.SheetNames.Monthly)]
    public bool Monthly { get; set; } = true;

    [JsonPropertyName("yearly")]
    [SheetOrder(13, SheetsConfig.SheetNames.Yearly)]
    public bool Yearly { get; set; } = true;

    [JsonPropertyName("setup")]
    [SheetOrder(14, SheetsConfig.SheetNames.Setup)]
    public bool Setup { get; set; } = true;
}
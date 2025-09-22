using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Google Sheets workbook.
/// This is a data transfer object that aggregates data from all sheets.
/// The order of properties in this class determines the sheet tab order.
/// </summary>
public class SheetEntity
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.SheetNames.Trips)]
    public List<TripEntity> Trips { get; set; } = [];

    [JsonPropertyName("shifts")]
    [SheetOrder(SheetsConfig.SheetNames.Shifts)]
    public List<ShiftEntity> Shifts { get; set; } = [];

    [JsonPropertyName("expenses")]
    [SheetOrder(SheetsConfig.SheetNames.Expenses)]
    public List<ExpenseEntity> Expenses { get; set; } = [];

    [JsonPropertyName("addresses")]
    [SheetOrder(SheetsConfig.SheetNames.Addresses)]
    public List<AddressEntity> Addresses { get; set; } = [];

    [JsonPropertyName("names")]
    [SheetOrder(SheetsConfig.SheetNames.Names)]
    public List<NameEntity> Names { get; set; } = [];

    [JsonPropertyName("places")]
    [SheetOrder(SheetsConfig.SheetNames.Places)]
    public List<PlaceEntity> Places { get; set; } = [];

    [JsonPropertyName("regions")]
    [SheetOrder(SheetsConfig.SheetNames.Regions)]
    public List<RegionEntity> Regions { get; set; } = [];

    [JsonPropertyName("services")]
    [SheetOrder(SheetsConfig.SheetNames.Services)]
    public List<ServiceEntity> Services { get; set; } = [];

    [JsonPropertyName("types")]
    [SheetOrder(SheetsConfig.SheetNames.Types)]
    public List<TypeEntity> Types { get; set; } = [];

    [JsonPropertyName("daily")]
    [SheetOrder(SheetsConfig.SheetNames.Daily)]
    public List<DailyEntity> Daily { get; set; } = [];

    [JsonPropertyName("weekdays")]
    [SheetOrder(SheetsConfig.SheetNames.Weekdays)]
    public List<WeekdayEntity> Weekdays { get; set; } = [];

    [JsonPropertyName("weekly")]
    [SheetOrder(SheetsConfig.SheetNames.Weekly)]
    public List<WeeklyEntity> Weekly { get; set; } = [];

    [JsonPropertyName("monthly")]
    [SheetOrder(SheetsConfig.SheetNames.Monthly)]
    public List<MonthlyEntity> Monthly { get; set; } = [];

    [JsonPropertyName("yearly")]
    [SheetOrder(SheetsConfig.SheetNames.Yearly)]
    public List<YearlyEntity> Yearly { get; set; } = [];

    [JsonPropertyName("setup")]
    [SheetOrder(SheetsConfig.SheetNames.Setup)]
    public List<SetupEntity> Setup { get; set; } = [];

    [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Constants;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Google Sheets workbook.
/// This is a data transfer object that aggregates data from all sheets.
/// Sheet order is determined by the declaration order in SheetsConfig.SheetNames.
/// </summary>
public class SheetEntity
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

    [JsonPropertyName("trips")]
    public List<TripEntity> Trips { get; set; } = [];

    [JsonPropertyName("shifts")]
    public List<ShiftEntity> Shifts { get; set; } = [];

    [JsonPropertyName("expenses")]
    public List<ExpenseEntity> Expenses { get; set; } = [];

    [JsonPropertyName("addresses")]
    public List<AddressEntity> Addresses { get; set; } = [];

    [JsonPropertyName("names")]
    public List<NameEntity> Names { get; set; } = [];

    [JsonPropertyName("places")]
    public List<PlaceEntity> Places { get; set; } = [];

    [JsonPropertyName("regions")]
    public List<RegionEntity> Regions { get; set; } = [];

    [JsonPropertyName("services")]
    public List<ServiceEntity> Services { get; set; } = [];

    [JsonPropertyName("types")]
    public List<TypeEntity> Types { get; set; } = [];

    [JsonPropertyName("daily")]
    public List<DailyEntity> Daily { get; set; } = [];

    [JsonPropertyName("weekdays")]
    public List<WeekdayEntity> Weekdays { get; set; } = [];

    [JsonPropertyName("weekly")]
    public List<WeeklyEntity> Weekly { get; set; } = [];

    [JsonPropertyName("monthly")]
    public List<MonthlyEntity> Monthly { get; set; } = [];

    [JsonPropertyName("yearly")]
    public List<YearlyEntity> Yearly { get; set; } = [];

    [JsonPropertyName("setup")]
    public List<SetupEntity> Setup { get; set; } = [];

    [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}
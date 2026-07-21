using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Holds every strongly-typed row collection for the Gig domain, nested under
/// <see cref="SheetEntity.Sheets"/> so domain sheet data can never collide with the reserved
/// top-level <c>Properties</c>/<c>Messages</c> members (a sheet could legitimately be named
/// "Properties" or "Messages"). Sheet order follows the declaration order in SheetsConfig.SheetNames.
/// </summary>
[ExcludeFromCodeCoverage]
public class GigSheets
{
    [JsonPropertyName("trips")]
    public List<TripEntity> Trips { get; set; } = [];

    [JsonPropertyName("shifts")]
    public List<ShiftEntity> Shifts { get; set; } = [];

    [JsonPropertyName("expenses")]
    public List<ExpenseEntity> Expenses { get; set; } = [];

    [JsonPropertyName("addresses")]
    public List<AddressEntity> Addresses { get; set; } = [];

    [JsonPropertyName("deliveries")]
    public List<DeliveryEntity> Deliveries { get; set; } = [];

    [JsonPropertyName("locations")]
    public List<LocationEntity> Locations { get; set; } = [];

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
}

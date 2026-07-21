using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Home.Entities;

/// <summary>
/// Holds every strongly-typed row collection for the Home domain, nested under
/// <see cref="SheetEntity.Sheets"/> so domain sheet data can never collide with the reserved
/// top-level <c>Properties</c>/<c>Messages</c> members. Sheet order follows
/// SheetsConfig.SheetUtilities.GetAllSheetNames().
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeSheets
{
    [JsonPropertyName("appliances")]
    public List<ApplianceEntity> Appliances { get; set; } = [];

    [JsonPropertyName("projects")]
    public List<ProjectEntity> Projects { get; set; } = [];

    [JsonPropertyName("maintenance")]
    public List<MaintenanceEntity> Maintenance { get; set; } = [];

    [JsonPropertyName("doors")]
    public List<DoorEntity> Doors { get; set; } = [];

    [JsonPropertyName("paints")]
    public List<PaintEntity> Paints { get; set; } = [];

    [JsonPropertyName("power")]
    public List<PowerEntity> Power { get; set; } = [];

    [JsonPropertyName("rooms")]
    public List<RoomEntity> Rooms { get; set; } = [];

    [JsonPropertyName("contacts")]
    public List<ContactEntity> Contacts { get; set; } = [];

    [JsonPropertyName("stats")]
    public List<StatEntity> Stats { get; set; } = [];
}

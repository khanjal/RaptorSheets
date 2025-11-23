using System.Text.Json;
using RaptorSheets.Core.Serialization;
using RaptorSheets.Gig.Entities;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Serialization;

public class ColumnAttributeSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public ColumnAttributeSerializationTests()
    {
        _options = JsonSerializerOptionsExtensions.CreateWithColumnAttributeNaming();
    }

    [Fact]
    public void TripEntity_ShouldSerializeWithColumnAttributeJsonNames()
    {
        // Arrange
        var trip = new TripEntity
        {
            RowId = 42,
            Pickup = "09:00 AM",
            Dropoff = "09:30 AM",
            OdometerStart = 12345.6m,
            OdometerEnd = 12360.5m,
            Distance = 14.9m
        };

        // Act
        var json = JsonSerializer.Serialize(trip, _options);

        // Assert - Should use Column attribute's jsonPropertyName
        Assert.Contains("\"pickupTime\":\"09:00 AM\"", json);
        Assert.Contains("\"dropoffTime\":\"09:30 AM\"", json);
        Assert.Contains("\"startOdometer\":12345.6", json);
        Assert.Contains("\"endOdometer\":12360.5", json);
        Assert.Contains("\"distance\":14.9", json);
        
        // Should not contain property names
        Assert.DoesNotContain("\"pickup\":", json);
        Assert.DoesNotContain("\"dropoff\":", json);
        Assert.DoesNotContain("\"odometerStart\":", json);
        Assert.DoesNotContain("\"odometerEnd\":", json);
    }

    [Fact]
    public void TripEntity_ShouldDeserializeWithColumnAttributeJsonNames()
    {
        // Arrange
        var json = """
        {
            "rowId": 42,
            "pickupTime": "09:00 AM",
            "dropoffTime": "09:30 AM",
            "startOdometer": 12345.6,
            "endOdometer": 12360.5,
            "distance": 14.9
        }
        """;

        // Act
        var trip = JsonSerializer.Deserialize<TripEntity>(json, _options);

        // Assert
        Assert.NotNull(trip);
        Assert.Equal(42, trip.RowId);
        Assert.Equal("09:00 AM", trip.Pickup);
        Assert.Equal("09:30 AM", trip.Dropoff);
        Assert.Equal(12345.6m, trip.OdometerStart);
        Assert.Equal(12360.5m, trip.OdometerEnd);
        Assert.Equal(14.9m, trip.Distance);
    }

    [Fact]
    public void TripEntity_PropertiesWithoutCustomJsonName_ShouldUseCamelCase()
    {
        // Arrange
        var trip = new TripEntity
        {
            RowId = 1,
            Date = "2024-01-15",
            Service = "DoorDash",
            Pay = 8.50m
        };

        // Act
        var json = JsonSerializer.Serialize(trip, _options);

        // Assert - Properties without custom jsonPropertyName should use camelCase
        Assert.Contains("\"date\":\"2024-01-15\"", json);
        Assert.Contains("\"service\":\"DoorDash\"", json);
        Assert.Contains("\"pay\":8.5", json);
    }
}

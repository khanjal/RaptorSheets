using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Tests.Data.Helpers;

internal class TestGigHelpers
{
    /// <summary>
    /// Loads static test data from JSON for legacy or specific unit tests.
    /// </summary>
    internal static SheetEntity? LoadSheetJson()
    {
        var path = JsonHelpers.GetDataJsonPath("ShiftWithTrips");
        var json = JsonHelpers.ReadJson(path);
        var sheetData = JsonHelpers.DeserializeJson<SheetEntity>(json);

        return sheetData;
    }

    /// <summary>
    /// Generate specific test data with known patterns for selective deletion testing.
    /// </summary>
    internal static SheetEntity GenerateSelectiveDeletionTestData(ActionTypeEnum actionType, int shiftStartId = 2, int tripStartId = 2)
    {
        var sheetEntity = new SheetEntity();
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        // Create 3 shifts with known patterns
        var shifts = new[]
        {
            new { ShiftId = shiftStartId, Number = 101, Service = "Uber", Region = "TestDelete", Trips = 4 },
            new { ShiftId = shiftStartId + 1, Number = 102, Service = "Lyft", Region = "TestKeep", Trips = 3 },
            new { ShiftId = shiftStartId + 2, Number = 103, Service = "DoorDash", Region = "TestDelete", Trips = 5 }
        };
        var currentTripId = tripStartId;
        foreach (var shiftTemplate in shifts)
        {
            // Create shift
            sheetEntity.Shifts.Add(new ShiftEntity 
            { 
                RowId = shiftTemplate.ShiftId, 
                Action = actionType.GetDescription(), 
                Date = date, 
                Number = shiftTemplate.Number, 
                Service = shiftTemplate.Service, 
                Start = DateTime.Now.ToString("T"),
                Region = shiftTemplate.Region,
                Note = $"{actionType.GetDescription()} - {shiftTemplate.Service} shift"
            });
            // Create trips for this shift
            for (int i = 0; i < shiftTemplate.Trips; i++)
            {
                var tripEntity = GenerateTrip();
                tripEntity.Action = actionType.GetDescription();
                tripEntity.RowId = currentTripId++;
                tripEntity.Date = date;
                tripEntity.Number = shiftTemplate.Number;
                tripEntity.Service = shiftTemplate.Service;
                tripEntity.Region = shiftTemplate.Region;
                tripEntity.Pickup = DateTime.Now.ToString("T");
                tripEntity.Dropoff = DateTime.Now.AddMinutes(10).ToString("T");
                tripEntity.Duration = "00:10:00.000";
                tripEntity.Place = $"Place_{shiftTemplate.Service}_{i + 1}";
                tripEntity.StartAddress = $"Start Address {i + 1}";
                tripEntity.Name = $"Customer_{shiftTemplate.Service}_{i + 1}";
                tripEntity.EndAddress = $"End Address {i + 1}";
                tripEntity.Note = $"{actionType.GetDescription()} - {shiftTemplate.Service} trip {i + 1}";
                sheetEntity.Trips.Add(tripEntity);
            }
        }
        return sheetEntity;
    }

    /// <summary>
    /// Simple trip entity generator for legacy/specific tests.
    /// </summary>
    private static TripEntity GenerateTrip()
    {
        var random = new Random();
        var pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()), 2);
        var tip = random.Next(1, 5);
        // Odometer logic: 30% chance to have odometer values
        decimal? odometerStart = random.NextDouble() < 0.3 ? random.Next(10000, 50000) : null;
        decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(1, 20) : (decimal?)null;
        decimal? distance = null;
        if (odometerStart.HasValue && odometerEnd.HasValue) {
            // 80% chance to use odometer diff as distance, 20% chance to override
            distance = random.NextDouble() < 0.8 ? odometerEnd - odometerStart : Math.Round((decimal)random.NextDouble() * 20, 1);
        } else {
            // 70% chance to have distance even if no odometer
            distance = random.NextDouble() < 0.7 ? Math.Round((decimal)random.NextDouble() * 20, 1) : null;
        }
        return new TripEntity {
            Type = "Pickup",
            Pay = pay,
            Tip = tip,
            OdometerStart = odometerStart,
            OdometerEnd = odometerEnd,
            Distance = distance
        };
    }
}

using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Tests.Data.Entities;

namespace RaptorSheets.Gig.Tests.Data.Helpers;

internal class TestGigHelpers
{
    internal static SheetEntity? LoadSheetJson()
    {
        var path = JsonHelpers.GetDataJsonPath("ShiftWithTrips");
        var json = JsonHelpers.ReadJson(path);
        var sheetData = JsonHelpers.DeserializeJson<SheetEntity>(json);

        return sheetData;
    }
    internal static SheetEntity GenerateShift(ActionTypeEnum actionType, int shiftStartId = 2, int tripStartId = 2)
    {
        // Get JSON data
        var services = JsonHelpers.LoadJsonData<List<string>>("services");
        var service = services?.GetRandomItem();
        var places = JsonHelpers.LoadJsonData<List<PlaceJsonEntity>>("places")!.Where(x => x.Services.Contains(service!)).ToList();
        var names = JsonHelpers.LoadJsonData<List<NameJsonEntity>>("names");

        // Create shift/trips
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var random = new Random();
        var randomNumber = random.Next(1, 99);

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity { 
            RowId = shiftStartId, 
            Action = actionType.GetDescription(), 
            Date = date, 
            Number = randomNumber, 
            Service = service!, 
            Start = DateTime.Now.ToString("T"), 
            Region = "Test",
            Note = actionType.GetDescription() }
        );

        // Add random amount of trips
        for (int i = tripStartId; i < random.Next(tripStartId + 1, tripStartId + 5); i++)
        {
            var place = places.GetRandomItem();
            var name = names!.GetRandomItem();

            var tripEntity = GenerateTrip();
            tripEntity.Action = actionType.GetDescription();
            tripEntity.RowId = i;
            tripEntity.Date = date;
            tripEntity.Number = randomNumber;
            tripEntity.Service = service!;
            tripEntity.Region = "Test";
            tripEntity.Pickup = DateTime.Now.ToString("T");
            tripEntity.Dropoff = DateTime.Now.AddMinutes(10).ToString("T");
            tripEntity.Duration = "00:10:00.000";
            tripEntity.Place = place.Name;
            tripEntity.StartAddress = place.Addresses.GetRandomItem();
            tripEntity.Name = name.Name;
            tripEntity.EndAddress = name.Address;
            tripEntity.Note = actionType.GetDescription();
            sheetEntity.Trips.Add(tripEntity);
        }

        return sheetEntity;
    }

    /// <summary>
    /// Generate multiple shifts with multiple trips for comprehensive testing
    /// </summary>
    /// <param name="actionType">The action type for all generated data</param>
    /// <param name="shiftStartId">Starting ID for shifts</param>
    /// <param name="tripStartId">Starting ID for trips</param>
    /// <param name="numberOfShifts">Number of shifts to generate (default: 5)</param>
    /// <param name="minTripsPerShift">Minimum trips per shift (default: 3)</param>
    /// <param name="maxTripsPerShift">Maximum trips per shift (default: 8)</param>
    /// <returns>SheetEntity with multiple shifts and trips</returns>
    internal static SheetEntity GenerateMultipleShifts(ActionTypeEnum actionType, int shiftStartId = 2, int tripStartId = 2, int numberOfShifts = 5, int minTripsPerShift = 3, int maxTripsPerShift = 8)
    {
        // Get JSON data
        var services = JsonHelpers.LoadJsonData<List<string>>("services");
        var places = JsonHelpers.LoadJsonData<List<PlaceJsonEntity>>("places")!;
        var names = JsonHelpers.LoadJsonData<List<NameJsonEntity>>("names");
        var regions = new[] { "Test", "Downtown", "Suburbs", "Airport", "University" };

        var random = new Random();
        var sheetEntity = new SheetEntity();
        var currentTripId = tripStartId;

        // Generate multiple shifts over different days
        for (int shiftIndex = 0; shiftIndex < numberOfShifts; shiftIndex++)
        {
            var service = services?.GetRandomItem();
            var placesForService = places.Where(x => x.Services.Contains(service!)).ToList();
            var date = DateTime.Now.AddDays(-numberOfShifts + shiftIndex).ToString("yyyy-MM-dd"); // Spread across different days
            var shiftNumber = random.Next(1, 99);
            var region = regions[random.Next(regions.Length)];

            // Create shift
            sheetEntity.Shifts.Add(new ShiftEntity { 
                RowId = shiftStartId + shiftIndex, 
                Action = actionType.GetDescription(), 
                Date = date, 
                Number = shiftNumber, 
                Service = service!, 
                Start = DateTime.Now.AddHours(random.Next(6, 22)).ToString("T"), // Random start time between 6 AM and 10 PM
                Region = region,
                Note = $"{actionType.GetDescription()} - Shift {shiftIndex + 1}"
            });

            // Generate trips for this shift
            var tripsForThisShift = random.Next(minTripsPerShift, maxTripsPerShift + 1);
            for (int tripIndex = 0; tripIndex < tripsForThisShift; tripIndex++)
            {
                var place = placesForService.GetRandomItem();
                var name = names!.GetRandomItem();
                var tripEntity = GenerateTrip();
                
                tripEntity.Action = actionType.GetDescription();
                tripEntity.RowId = currentTripId++;
                tripEntity.Date = date;
                tripEntity.Number = shiftNumber;
                tripEntity.Service = service!;
                tripEntity.Region = region;
                tripEntity.Pickup = DateTime.Now.AddHours(random.Next(6, 22)).AddMinutes(random.Next(0, 60)).ToString("T");
                tripEntity.Dropoff = DateTime.Now.AddHours(random.Next(6, 22)).AddMinutes(random.Next(0, 60)).ToString("T");
                tripEntity.Duration = $"00:{random.Next(5, 45):D2}:00.000"; // Random duration between 5-45 minutes
                tripEntity.Place = place.Name;
                tripEntity.StartAddress = place.Addresses.GetRandomItem();
                tripEntity.Name = name.Name;
                tripEntity.EndAddress = name.Address;
                tripEntity.Note = $"{actionType.GetDescription()} - Trip {tripIndex + 1} of Shift {shiftIndex + 1}";
                
                sheetEntity.Trips.Add(tripEntity);
            }
        }

        return sheetEntity;
    }

    /// <summary>
    /// Generate specific test data with known patterns for selective deletion testing
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

    private static TripEntity GenerateTrip()
    {
        var random = new Random();
        var pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()), 2);
        var distance = Math.Round(random.Next(0, 20) + new decimal(random.NextDouble()), 1);
        var tip = random.Next(1, 5);

        return new TripEntity { Type = "Pickup", Pay = pay, Tip = tip, Distance = distance };
    }
}

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

        // Generate random start/finish times
        var shiftStart = DateTime.Today.AddHours(random.Next(6, 16)).AddMinutes(random.Next(0, 60));
        var shiftDuration = TimeSpan.FromMinutes(random.Next(240, 600)); // 4-10 hours
        var shiftFinish = shiftStart.Add(shiftDuration);

        // Generate random active duration less than shift duration
        var activeMinutes = random.Next(30, (int)shiftDuration.TotalMinutes); // at least 30 min
        var activeDuration = TimeSpan.FromMinutes(activeMinutes);

        // Decide if this shift will have trips (85% chance) or not (15% chance)
        bool hasTrips = random.NextDouble() >= 0.15;
        int tripCount = hasTrips ? random.Next(1, 5) : 0;
        int tripsValue = tripCount;
        if (hasTrips && random.NextDouble() < 0.2)
            tripsValue += random.Next(-1, 2);

        // Always set distance for no-trip shifts
        decimal? odometerStart = random.NextDouble() < 0.7 ? random.Next(10000, 50000) : null;
        decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(10, 100) : (decimal?)null;
        decimal? distance = (odometerStart.HasValue && odometerEnd.HasValue) ? odometerEnd - odometerStart : Math.Round((decimal)random.NextDouble() * 20 + 1, 2);

        // Randomize pay/tip for shift
        decimal? pay = Math.Round((decimal)random.NextDouble() * 200 + 20, 2);
        decimal? tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 40, 2) : null;
        decimal? bonus = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 50, 2) : null;
        decimal? cash = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 100, 2) : null;

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity {
            RowId = shiftStartId,
            Action = actionType.GetDescription(),
            Date = date,
            Number = randomNumber,
            Service = service!,
            Start = shiftStart.ToString("T"),
            Finish = shiftFinish.ToString("T"),
            Active = activeDuration.ToString(@"hh\:mm\:ss"),
            Time = shiftDuration.ToString(),
            Region = "Test",
            Note = actionType.GetDescription(),
            Bonus = hasTrips ? bonus : (random.NextDouble() < 0.15 ? bonus : null),
            Cash = hasTrips ? cash : (random.NextDouble() < 0.15 ? cash : null),
            OdometerStart = odometerStart,
            OdometerEnd = odometerEnd,
            Distance = distance,
            Pay = pay,
            Tip = tip,
            Trips = hasTrips ? Math.Max(0, tripsValue) : random.Next(1, 5),
            Omit = random.NextDouble() < 0.08 // 8% chance to omit
        });

        // Only add trips if hasTrips is true
        if (hasTrips)
        {
            for (int i = tripStartId; i < tripStartId + tripCount; i++)
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

                // Assign pay/tip to some trips (e.g., 70% chance)
                if (random.NextDouble() < 0.7)
                {
                    tripEntity.Pay = Math.Round((decimal)random.NextDouble() * 40 + 5, 2);
                    tripEntity.Tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 10, 2) : null;
                }

                // Rare fields
                if (random.NextDouble() < 0.05)
                    tripEntity.Exclude = true;
                if (random.NextDouble() < 0.1)
                    tripEntity.Cash = Math.Round((decimal)random.NextDouble() * 30 + 1, 2);
                if (random.NextDouble() < 0.1)
                    tripEntity.Bonus = Math.Round((decimal)random.NextDouble() * 20 + 1, 2);
                if (random.NextDouble() < 0.1)
                {
                    var units = new[] { "A", "B", "C", "D", "E" };
                    tripEntity.EndUnit = units[random.Next(units.Length)];
                }
                if (random.NextDouble() < 0.1)
                    tripEntity.OrderNumber = random.Next(100000, 999999).ToString();

                sheetEntity.Trips.Add(tripEntity);
            }
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

        for (int shiftIndex = 0; shiftIndex < numberOfShifts; shiftIndex++)
        {
            var service = services?.GetRandomItem();
            var placesForService = places.Where(x => x.Services.Contains(service!)).ToList();
            var date = DateTime.Now.AddDays(-numberOfShifts + shiftIndex).ToString("yyyy-MM-dd");
            var shiftNumber = random.Next(1, 99);
            var region = regions[random.Next(regions.Length)];

            // Random start/finish
            var shiftStart = DateTime.Today.AddHours(random.Next(6, 16)).AddMinutes(random.Next(0, 60));
            var shiftDuration = TimeSpan.FromMinutes(random.Next(240, 600));
            var shiftFinish = shiftStart.Add(shiftDuration);
            var activeMinutes = random.Next(30, (int)shiftDuration.TotalMinutes);
            var activeDuration = TimeSpan.FromMinutes(activeMinutes);
            
            // Decide if this shift will have trips (85% chance) or not (15% chance)
            bool hasTrips = random.NextDouble() >= 0.15;
            int tripCount = hasTrips ? random.Next(minTripsPerShift, maxTripsPerShift + 1) : 0;
            int tripsValue = tripCount;
            if (hasTrips && random.NextDouble() < 0.2)
                tripsValue += random.Next(-1, 2);

            decimal? odometerStart = random.NextDouble() < 0.7 ? random.Next(10000, 50000) : null;
            decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(10, 100) : (decimal?)null;
            decimal? distance = (odometerStart.HasValue && odometerEnd.HasValue) ? odometerEnd - odometerStart : Math.Round((decimal)random.NextDouble() * 20 + 1, 2);

            decimal? pay = Math.Round((decimal)random.NextDouble() * 200 + 20, 2);
            decimal? tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 40, 2) : null;
            decimal? bonus = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 50, 2) : null;
            decimal? cash = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 100, 2) : null;

            sheetEntity.Shifts.Add(new ShiftEntity {
                RowId = shiftStartId + shiftIndex,
                Action = actionType.GetDescription(),
                Date = date,
                Number = shiftNumber,
                Service = service!,
                Start = shiftStart.ToString("T"),
                Finish = shiftFinish.ToString("T"),
                Active = activeDuration.ToString(@"hh\:mm\:ss"),
                Time = shiftDuration.ToString(),
                Region = region,
                Note = $"{actionType.GetDescription()} - Shift {shiftIndex + 1}",
                Bonus = hasTrips ? bonus : (random.NextDouble() < 0.15 ? bonus : null),
                Cash = hasTrips ? cash : (random.NextDouble() < 0.15 ? cash : null),
                OdometerStart = odometerStart,
                OdometerEnd = odometerEnd,
                Distance = distance,
                Pay = pay,
                Tip = tip,
                Trips = hasTrips ? Math.Max(0, tripsValue) : random.Next(1, 5),
                Omit = random.NextDouble() < 0.08
            });

            if (hasTrips)
            {
                for (int tripIndex = 0; tripIndex < tripCount; tripIndex++)
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
                    tripEntity.Duration = $"00:{random.Next(5, 45):D2}:00.000";
                    tripEntity.Place = place.Name;
                    tripEntity.StartAddress = place.Addresses.GetRandomItem();
                    tripEntity.Name = name.Name;
                    tripEntity.EndAddress = name.Address;
                    tripEntity.Note = $"{actionType.GetDescription()} - Trip {tripIndex + 1} of Shift {shiftIndex + 1}";

                    if (random.NextDouble() < 0.7)
                    {
                        tripEntity.Pay = Math.Round((decimal)random.NextDouble() * 40 + 5, 2);
                        tripEntity.Tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 10, 2) : null;
                    }

                    if (random.NextDouble() < 0.05)
                        tripEntity.Exclude = true;
                    if (random.NextDouble() < 0.1)
                        tripEntity.Cash = Math.Round((decimal)random.NextDouble() * 30 + 1, 2);
                    if (random.NextDouble() < 0.1)
                        tripEntity.Bonus = Math.Round((decimal)random.NextDouble() * 20 + 1, 2);
                    if (random.NextDouble() < 0.1)
                    {
                        var units = new[] { "A", "B", "C", "D", "E" };
                        tripEntity.EndUnit = units[random.Next(units.Length)];
                    }
                    if (random.NextDouble() < 0.1)
                        tripEntity.OrderNumber = random.Next(100000, 999999).ToString();

                    sheetEntity.Trips.Add(tripEntity);
                }
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

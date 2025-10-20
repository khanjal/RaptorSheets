using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper class for creating demo/sample data for RaptorGig spreadsheets.
/// Can be used to create a new demo spreadsheet or populate an existing one.
/// </summary>
public static class DemoHelpers
{
    /// <summary>
    /// Generates realistic sample gig data for demonstration purposes.
    /// Creates shifts, trips, and expenses across a date range.
    /// </summary>
    /// <param name="startDate">Start date for the demo data</param>
    /// <param name="endDate">End date for the demo data</param>
    /// <returns>SheetEntity populated with realistic demo data</returns>
    public static SheetEntity GenerateDemoData(DateTime startDate, DateTime endDate)
    {
        var services = new List<string> { "DoorDash", "Uber Eats", "Grubhub", "Instacart", "Amazon Flex", "Shipt" };
        var regions = new List<string> 
        { 
            "San Francisco", "Oakland", "Berkeley", "San Jose", "Palo Alto", 
            "Sunnyvale", "Mountain View", "Redwood City", "Fremont", "Hayward"
        };
        var expenseCategories = new List<string> { "Fuel", "Maintenance", "Car Wash", "Supplies", "Parking", "Tolls", "Phone" };
        
        var random = new Random();
        var sheetEntity = new SheetEntity();
        var shiftId = 2; // Start at 2 because row 1 is for headers
        var tripId = 2;  // Start at 2 because row 1 is for headers
        var expenseId = 2; // Start at 2 because row 1 is for headers
        var serviceDayShiftNumber = new Dictionary<(string, string), int>(); // (service, date) -> shift number

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Generate 1-3 shifts per day (some days might have no shifts)
            int numShiftsToday = random.NextDouble() < 0.85 ? random.Next(1, 4) : 0; // 85% chance of working
            
            var servicesToday = services.OrderBy(_ => random.Next())
                .Take(random.Next(1, Math.Min(numShiftsToday + 1, services.Count))).ToList();
            
            for (int s = 0; s < numShiftsToday; s++)
            {
                // Pick a service for this shift
                string service = servicesToday[random.Next(servicesToday.Count)];
                if (!serviceDayShiftNumber.ContainsKey((service, date.ToString("yyyy-MM-dd"))))
                    serviceDayShiftNumber[(service, date.ToString("yyyy-MM-dd"))] = 1;
                else
                    serviceDayShiftNumber[(service, date.ToString("yyyy-MM-dd"))]++;
                int shiftNumber = serviceDayShiftNumber[(service, date.ToString("yyyy-MM-dd"))];

                // Pick a region
                string region = regions[random.Next(regions.Count)];

                // Generate realistic shift times
                var shiftStart = date.AddHours(random.Next(6, 16)).AddMinutes(random.Next(0, 60));
                var shiftDuration = TimeSpan.FromMinutes(random.Next(120, 480)); // 2-8 hours
                var shiftFinish = shiftStart.Add(shiftDuration);
                var activeMinutes = random.Next(30, (int)shiftDuration.TotalMinutes);
                var activeDuration = TimeSpan.FromMinutes(activeMinutes);

                // Decide if this shift will have trips (85% chance) or not (15% chance for no-trip shifts)
                bool hasTrips = random.NextDouble() >= 0.15;
                int tripCount = hasTrips ? random.Next(2, 10) : 0;
                int tripsValue = tripCount;
                if (hasTrips && random.NextDouble() < 0.2)
                    tripsValue += random.Next(-1, 2); // Sometimes shift.Trips differs slightly from actual trip count

                // Odometer and distance
                decimal? odometerStart = random.NextDouble() < 0.7 ? random.Next(10000, 90000) : null;
                decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(10, 100) : (decimal?)null;
                decimal? distance = (odometerStart.HasValue && odometerEnd.HasValue) 
                    ? odometerEnd - odometerStart 
                    : Math.Round((decimal)random.NextDouble() * 20 + 1, 2);

                // Earnings
                decimal? pay = Math.Round((decimal)random.NextDouble() * 200 + 20, 2);
                decimal? tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 40, 2) : null;
                decimal? bonus = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 50, 2) : null;
                decimal? cash = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 100, 2) : null;

                sheetEntity.Shifts.Add(new ShiftEntity 
                {
                    RowId = shiftId++,
                    Action = ActionTypeEnum.INSERT.GetDescription(),
                    Date = date.ToString("yyyy-MM-dd"),
                    Number = shiftNumber,
                    Service = service,
                    Start = shiftStart.ToString("T"),
                    Finish = shiftFinish.ToString("T"),
                    Active = activeDuration.ToString(@"hh\:mm\:ss"),
                    Time = shiftDuration.ToString(),
                    Region = region,
                    Note = $"Demo {service} shift",
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

                // Generate trips for this shift if applicable
                if (hasTrips)
                {
                    for (int tripIndex = 0; tripIndex < tripCount; tripIndex++)
                    {
                        var tripEntity = GenerateDemoTrip(
                            tripId++, 
                            date, 
                            shiftNumber, 
                            service, 
                            region, 
                            shiftStart, 
                            shiftDuration, 
                            tripIndex + 1);
                        
                        sheetEntity.Trips.Add(tripEntity);
                    }
                }
            }
            
            // Generate 0-2 expenses per day (expenses don't happen every day)
            int numExpensesToday = random.NextDouble() < 0.4 ? random.Next(1, 3) : 0; // 40% chance of expenses
            
            for (int e = 0; e < numExpensesToday; e++)
            {
                var category = expenseCategories[random.Next(expenseCategories.Count)];
                var amount = category switch
                {
                    "Fuel" => Math.Round((decimal)random.NextDouble() * 40 + 20, 2),
                    "Maintenance" => Math.Round((decimal)random.NextDouble() * 100 + 30, 2),
                    "Car Wash" => Math.Round((decimal)random.NextDouble() * 15 + 10, 2),
                    "Supplies" => Math.Round((decimal)random.NextDouble() * 30 + 5, 2),
                    "Parking" => Math.Round((decimal)random.NextDouble() * 10 + 3, 2),
                    "Tolls" => Math.Round((decimal)random.NextDouble() * 8 + 2, 2),
                    "Phone" => Math.Round((decimal)random.NextDouble() * 50 + 30, 2),
                    _ => Math.Round((decimal)random.NextDouble() * 50 + 10, 2)
                };
                
                sheetEntity.Expenses.Add(new ExpenseEntity
                {
                    RowId = expenseId++,
                    Action = ActionTypeEnum.INSERT.GetDescription(),
                    Date = date,
                    Category = category,
                    Name = $"{category} - Demo",
                    Amount = amount,
                    Description = $"Demo {category.ToLower()} expense"
                });
            }
        }
        
        return sheetEntity;
    }

    /// <summary>
    /// Generates a single demo trip with realistic data.
    /// </summary>
    private static TripEntity GenerateDemoTrip(
        int rowId, 
        DateTime date, 
        int shiftNumber, 
        string service, 
        string region, 
        DateTime shiftStart, 
        TimeSpan shiftDuration,
        int tripNumber)
    {
        var random = new Random();
        
        // Trip types vary by service
        var tripType = service switch
        {
            "Uber Eats" or "DoorDash" or "Grubhub" => random.NextDouble() < 0.7 ? "Delivery" : "Pickup",
            "Instacart" or "Shipt" => "Delivery",
            "Amazon Flex" => "Delivery",
            _ => random.NextDouble() < 0.5 ? "Pickup" : "Delivery"
        };

        // Trip timing within the shift
        var tripStart = shiftStart.AddMinutes(random.Next(0, (int)shiftDuration.TotalMinutes));
        var tripDuration = TimeSpan.FromMinutes(random.Next(5, 45));
        var tripEnd = tripStart.Add(tripDuration);

        // Odometer logic: 30% chance to have odometer values
        decimal? odometerStart = random.NextDouble() < 0.3 ? random.Next(10000, 50000) : null;
        decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(1, 20) : (decimal?)null;
        decimal? distance = null;

        if (odometerStart.HasValue && odometerEnd.HasValue) 
        {
            // 80% chance to use odometer diff as distance, 20% chance to override
            distance = random.NextDouble() < 0.8 
                ? odometerEnd - odometerStart 
                : Math.Round((decimal)random.NextDouble() * 20, 1);
        } 
        else 
        {
            // 70% chance to have distance even if no odometer
            distance = random.NextDouble() < 0.7 ? Math.Round((decimal)random.NextDouble() * 20, 1) : null;
        }

        // Earnings vary by service
        var basePay = service switch
        {
            "DoorDash" or "Uber Eats" => (decimal)random.NextDouble() * 8 + 2,
            "Grubhub" => (decimal)random.NextDouble() * 10 + 3,
            "Instacart" or "Shipt" => (decimal)random.NextDouble() * 15 + 7,
            "Amazon Flex" => (decimal)random.NextDouble() * 20 + 15,
            _ => (decimal)random.NextDouble() * 10 + 5
        };

        var pay = Math.Round(basePay, 2);
        decimal? tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 10, 2) : (decimal?)null;
        decimal? bonus = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 20 + 1, 2) : (decimal?)null;
        decimal? cash = random.NextDouble() < 0.1 ? Math.Round((decimal)random.NextDouble() * 30 + 1, 2) : (decimal?)null;

        // Real restaurant and fast food places
        var restaurantPlaces = new[] 
        { 
            "McDonald's", "Chipotle", "Starbucks", "Panera Bread", "Taco Bell", 
            "Subway", "Panda Express", "In-N-Out Burger", "Chick-fil-A", "Olive Garden",
            "Red Lobster", "The Cheesecake Factory", "P.F. Chang's", "Five Guys", "Shake Shack",
            "Wendy's", "KFC", "Popeyes", "Buffalo Wild Wings", "Applebee's"
        };

        // Realistic street addresses
        var sampleAddresses = new[] 
        { 
            "123 Market St", "456 Mission St", "789 Main St", "321 Broadway", "654 University Ave", 
            "987 El Camino Real", "147 Castro St", "258 Valencia St", "369 Geary St", "741 Post St",
            "852 Van Ness Ave", "963 Divisadero St", "159 Hayes St", "753 Polk St", "951 Columbus Ave"
        };

        // First name + Last initial format
        var firstNames = new[] 
        { 
            "John", "Sarah", "Michael", "Emily", "David", "Jessica", "Chris", "Ashley", 
            "Ryan", "Amanda", "Kevin", "Jennifer", "Brian", "Lauren", "Daniel", "Rachel",
            "Matthew", "Michelle", "James", "Nicole", "Andrew", "Stephanie", "Jason", "Megan"
        };
        
        // Use GoogleConfig.ColumnLetters for last initial
        var columnLetters = GoogleConfig.ColumnLetters;
        var lastInitial = columnLetters[random.Next(columnLetters.Length)].ToString();
        var customerName = $"{firstNames[random.Next(firstNames.Length)]} {lastInitial}.";

        var tripEntity = new TripEntity
        {
            RowId = rowId,
            Action = ActionTypeEnum.INSERT.GetDescription(),
            Date = date.ToString("yyyy-MM-dd"),
            Number = shiftNumber,
            Service = service,
            Region = region,
            Type = tripType,
            Pickup = tripStart.ToString("T"),
            Dropoff = tripEnd.ToString("T"),
            Duration = tripDuration.ToString(@"hh\:mm\:ss\.fff"),
            Place = restaurantPlaces[random.Next(restaurantPlaces.Length)],
            StartAddress = sampleAddresses[random.Next(sampleAddresses.Length)],
            Name = customerName,
            EndAddress = sampleAddresses[random.Next(sampleAddresses.Length)],
            Pay = pay,
            Tip = tip,
            Bonus = bonus,
            Cash = cash,
            OdometerStart = odometerStart,
            OdometerEnd = odometerEnd,
            Distance = distance,
            Note = $"Demo trip {tripNumber}",
            Exclude = random.NextDouble() < 0.05, // 5% chance to exclude
            OrderNumber = random.NextDouble() < 0.1 ? random.Next(100000, 999999).ToString() : null
        };

        // Occasionally add unit numbers
        if (random.NextDouble() < 0.1)
        {
            var units = new[] { "A", "B", "C", "D", "E", "101", "202", "303", "Apt 5", "Suite 12" };
            tripEntity.EndUnit = units[random.Next(units.Length)];
        }

        return tripEntity;
    }
}

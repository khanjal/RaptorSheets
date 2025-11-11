using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using System.Globalization;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper class for creating demo/sample data for RaptorGig spreadsheets.
/// Can be used to create a new demo spreadsheet or populate an existing one.
/// </summary>
public static class DemoHelpers
{
    private const string DateFormat = "yyyy-MM-dd"; // Define constant for repeated date format

    public static string FormatDate(DateTime date)
    {
        return date.ToString(DateFormat);
    }

    public static DateTime ParseDate(string date)
    {
        return DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Context object for managing ID generation during demo data creation.
    /// </summary>
    private class DemoIdContext
    {
        public int ShiftId { get; set; } = 2;  // Start at 2 because row 1 is for headers
        public int TripId { get; set; } = 2;   // Start at 2 because row 1 is for headers
        public int ExpenseId { get; set; } = 2; // Start at 2 because row 1 is for headers
    }

    /// <summary>
    /// Context object for shift generation parameters.
    /// </summary>
    private class ShiftGenerationContext
    {
        public required Random Random { get; init; }
        public required SheetEntity SheetEntity { get; init; }
        public required DateTime Date { get; init; }
        public required List<string> Services { get; init; }
        public required List<string> Regions { get; init; }
        public required Dictionary<(string, string), int> ServiceDayShiftNumber { get; init; }
    }

    /// <summary>
    /// Context object for trip generation parameters.
    /// </summary>
    private record TripGenerationContext
    {
        public required Random Random { get; init; }
        public required DateTime Date { get; init; }
        public required int ShiftNumber { get; init; }
        public required string Service { get; init; }
        public required string Region { get; init; }
        public required DateTime ShiftStart { get; init; }
        public required TimeSpan ShiftDuration { get; init; }
        public required int TripNumber { get; init; }
    }

    /// <summary>
    /// Generates realistic sample gig data for demonstration purposes.
    /// Creates shifts, trips, and expenses across a date range.
    /// </summary>
    /// <param name="startDate">Start date for the demo data</param>
    /// <param name="endDate">End date for the demo data</param>
    /// <param name="seed">Optional seed for Random to enable deterministic/reproducible demo data (useful for testing)</param>
    /// <returns>SheetEntity populated with realistic demo data</returns>
    public static SheetEntity GenerateDemoData(DateTime startDate, DateTime endDate, int? seed = null)
    {
        // SonarQube S2245: Using Random is safe here - this generates demo/sample data, not security-sensitive values
        // Optional seed parameter allows deterministic generation for testing
        #pragma warning disable S2245
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        #pragma warning restore S2245
        
        var sheetEntity = new SheetEntity();
        var idContext = new DemoIdContext();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            GenerateDailyShiftsAndTrips(random, sheetEntity, date, idContext);
            GenerateDailyExpenses(random, sheetEntity, date, idContext);
        }
        
        return sheetEntity;
    }

    /// <summary>
    /// Generates shifts and associated trips for a single day.
    /// </summary>
    private static void GenerateDailyShiftsAndTrips(
        Random random, 
        SheetEntity sheetEntity, 
        DateTime date,
        DemoIdContext idContext)
    {
        // Services from CSV data: DoorDash (most common), Uber, Instacart
        var services = new List<string> { "DoorDash", "Uber Eats", "Grubhub", "Instacart", "Shipt" };
        var regions = new List<string> 
        { 
            "Bay Area", "North Bay", "East Bay", "South Bay", "Peninsula", 
            "Central Valley", "North County", "South County", "Metro", "Downtown"
        };
        
        var serviceDayShiftNumber = new Dictionary<(string, string), int>();

        // Generate 1-3 shifts per day (some days might have no shifts)
        int numShiftsToday = random.NextDouble() < 0.85 ? random.Next(1, 4) : 0; // 85% chance of working
        
        var servicesToday = services.OrderBy(_ => random.Next())
            .Take(random.Next(1, Math.Min(numShiftsToday + 1, services.Count))).ToList();
        
        var context = new ShiftGenerationContext
        {
            Random = random,
            SheetEntity = sheetEntity,
            Date = date,
            Services = servicesToday,
            Regions = regions,
            ServiceDayShiftNumber = serviceDayShiftNumber
        };

        for (int s = 0; s < numShiftsToday; s++)
        {
            GenerateSingleShiftWithTrips(context, idContext);
        }
    }

    /// <summary>
    /// Generates a single shift with associated trips.
    /// </summary>
    private static void GenerateSingleShiftWithTrips(
        ShiftGenerationContext context,
        DemoIdContext idContext)
    {
        // Pick a service for this shift
        string service = context.Services[context.Random.Next(context.Services.Count)];
        int shiftNumber = GetOrIncrementShiftNumber(context.ServiceDayShiftNumber, service, context.Date);

        // Pick a region
        string region = context.Regions[context.Random.Next(context.Regions.Count)];

        // Generate shift timing data
        var (shiftStart, shiftDuration, shiftFinish, activeDuration) = GenerateShiftTiming(context.Random, context.Date);

        // Determine trip configuration
        var (hasTrips, tripCount, tripsValue) = DetermineShiftTrips(context.Random);

        // Generate shift financial and travel data
        var shiftData = GenerateShiftData(context.Random, hasTrips);

        context.SheetEntity.Shifts.Add(new ShiftEntity 
        {
            RowId = idContext.ShiftId++,
            Action = ActionTypeEnum.INSERT.GetDescription(),
            Date = context.Date.ToString("yyyy-MM-dd"),
            Number = shiftNumber,
            Service = service,
            Start = shiftStart.ToString("T"),
            Finish = shiftFinish.ToString("T"),
            Active = activeDuration.ToString(@"hh\:mm\:ss"),
            Time = shiftDuration.ToString(),
            Region = region,
            Note = $"Demo {service} shift",
            Bonus = shiftData.Bonus,
            Cash = shiftData.Cash,
            OdometerStart = shiftData.OdometerStart,
            OdometerEnd = shiftData.OdometerEnd,
            Distance = shiftData.Distance,
            Pay = shiftData.Pay,
            Tip = shiftData.Tip,
            Trips = tripsValue,
            Omit = context.Random.NextDouble() < 0.08 // 8% chance to omit
        });

        // Generate trips for this shift if applicable
        if (hasTrips)
        {
            var tripContext = new TripGenerationContext
            {
                Random = context.Random,
                Date = context.Date,
                ShiftNumber = shiftNumber,
                Service = service,
                Region = region,
                ShiftStart = shiftStart,
                ShiftDuration = shiftDuration,
                TripNumber = 0 // Will be set in loop
            };

            for (int tripIndex = 0; tripIndex < tripCount; tripIndex++)
            {
                tripContext = tripContext with { TripNumber = tripIndex + 1 };
                var tripEntity = GenerateDemoTrip(tripContext, idContext.TripId++);
                context.SheetEntity.Trips.Add(tripEntity);
            }
        }
    }

    /// <summary>
    /// Gets or increments the shift number for a given service and date.
    /// </summary>
    private static int GetOrIncrementShiftNumber(
        Dictionary<(string, string), int> serviceDayShiftNumber,
        string service,
        DateTime date)
    {
        var key = (service, date.ToString("yyyy-MM-dd"));
        if (!serviceDayShiftNumber.ContainsKey(key))
        {
            serviceDayShiftNumber[key] = 1;
        }
        else
        {
            serviceDayShiftNumber[key]++;
        }
        return serviceDayShiftNumber[key];
    }

    /// <summary>
    /// Generates realistic shift timing data.
    /// </summary>
    private static (DateTime shiftStart, TimeSpan shiftDuration, DateTime shiftFinish, TimeSpan activeDuration) 
        GenerateShiftTiming(Random random, DateTime date)
    {
        var shiftStart = date.AddHours(random.Next(6, 16)).AddMinutes(random.Next(0, 60));
        var shiftDuration = TimeSpan.FromMinutes(random.Next(120, 480)); // 2-8 hours
        var shiftFinish = shiftStart.Add(shiftDuration);
        var activeMinutes = random.Next(30, (int)shiftDuration.TotalMinutes);
        var activeDuration = TimeSpan.FromMinutes(activeMinutes);

        return (shiftStart, shiftDuration, shiftFinish, activeDuration);
    }

    /// <summary>
    /// Determines whether the shift has trips and how many.
    /// </summary>
    private static (bool hasTrips, int tripCount, int tripsValue) DetermineShiftTrips(Random random)
    {
        // Decide if this shift will have trips (85% chance) or not (15% chance for no-trip shifts)
        bool hasTrips = random.NextDouble() >= 0.15;
        int tripCount = hasTrips ? random.Next(2, 10) : 0;
        int tripsValue = tripCount;
        
        if (hasTrips && random.NextDouble() < 0.2)
        {
            // Sometimes shift.Trips differs slightly from actual trip count
            tripsValue += random.Next(-1, 2);
        }
        
        if (!hasTrips)
        {
            tripsValue = random.Next(1, 5);
        }

        return (hasTrips, tripCount, Math.Max(0, tripsValue));
    }

    /// <summary>
    /// Generates shift financial and travel data.
    /// </summary>
    private static ShiftDataResult GenerateShiftData(Random random, bool hasTrips)
    {
        // Odometer and distance - based on CSV data showing 10-80 mile shifts
        decimal? odometerStart = random.NextDouble() < 0.7 ? random.Next(10000, 90000) : null;
        decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(10, 80) : null;
        decimal? distance = (odometerStart.HasValue && odometerEnd.HasValue) 
            ? odometerEnd - odometerStart 
            : Math.Round((decimal)random.NextDouble() * 70 + 10, 1);

        // Earnings - based on CSV showing $15-120 pay per shift, $5-50 tips
        decimal? pay = Math.Round((decimal)random.NextDouble() * 105 + 15, 2);
        decimal? tip = random.NextDouble() < 0.85 ? Math.Round((decimal)random.NextDouble() * 45 + 5, 2) : null;
        decimal? bonus = random.NextDouble() < 0.15 ? Math.Round((decimal)random.NextDouble() * 3 + 1, 2) : null;
        decimal? cash = random.NextDouble() < 0.05 ? Math.Round((decimal)random.NextDouble() * 10 + 2, 2) : null;

        // Adjust bonus and cash for non-trip shifts
        if (!hasTrips)
        {
            bonus = random.NextDouble() < 0.15 ? bonus : null;
            cash = random.NextDouble() < 0.15 ? cash : null;
        }

        return new ShiftDataResult
        {
            OdometerStart = odometerStart,
            OdometerEnd = odometerEnd,
            Distance = distance,
            Pay = pay,
            Tip = tip,
            Bonus = bonus,
            Cash = cash
        };
    }

    /// <summary>
    /// Container for shift financial and travel data.
    /// </summary>
    private record ShiftDataResult
    {
        public decimal? OdometerStart { get; init; }
        public decimal? OdometerEnd { get; init; }
        public decimal? Distance { get; init; }
        public decimal? Pay { get; init; }
        public decimal? Tip { get; init; }
        public decimal? Bonus { get; init; }
        public decimal? Cash { get; init; }
    }
    
    /// <summary>
    /// Generates expenses for a single day.
    /// </summary>
    private static void GenerateDailyExpenses(
        Random random, 
        SheetEntity sheetEntity, 
        DateTime date,
        DemoIdContext idContext)
    {
        var expenseCategories = new List<string> { "Fuel", "Maintenance", "Car Wash", "Supplies", "Parking", "Tolls", "Phone" };
        
        // Generate 0-2 expenses per day (expenses don't happen every day)
        int numExpensesToday = random.NextDouble() < 0.4 ? random.Next(1, 3) : 0; // 40% chance of expenses
        
        for (int e = 0; e < numExpensesToday; e++)
        {
            var category = expenseCategories[random.Next(expenseCategories.Count)];
            var amount = category switch
            {
                "Fuel" => Math.Round((decimal)random.NextDouble() * 30 + 25, 2),      // $25-$55
                "Maintenance" => Math.Round((decimal)random.NextDouble() * 150 + 50, 2), // $50-$200
                "Car Wash" => Math.Round((decimal)random.NextDouble() * 10 + 8, 2),   // $8-$18
                "Supplies" => Math.Round((decimal)random.NextDouble() * 20 + 5, 2),   // $5-$25
                "Parking" => Math.Round((decimal)random.NextDouble() * 12 + 3, 2),    // $3-$15
                "Tolls" => Math.Round((decimal)random.NextDouble() * 6 + 2, 2),       // $2-$8
                "Phone" => Math.Round((decimal)random.NextDouble() * 40 + 40, 2),     // $40-$80 (monthly)
                _ => Math.Round((decimal)random.NextDouble() * 50 + 10, 2)
            };
            
            sheetEntity.Expenses.Add(new ExpenseEntity
            {
                RowId = idContext.ExpenseId++,
                Action = ActionTypeEnum.INSERT.GetDescription(),
                Date = date.ToString("yyyy-MM-dd"),  // Convert DateTime to string format
                Category = category,
                Name = $"{category} - Demo",
                Amount = amount,
                Description = $"Demo {category.ToLower()} expense"
            });
        }
    }

    /// <summary>
    /// Generates a single demo trip with realistic data.
    /// </summary>
    private static TripEntity GenerateDemoTrip(
        TripGenerationContext context,
        int rowId)
    {
        // Trip types vary by service
        var tripType = DetermineTripType(context.Service);

        // Trip timing within the shift
        var (tripStart, tripDuration, tripEnd) = GenerateTripTiming(context.Random, context.ShiftStart, context.ShiftDuration);

        // Generate trip travel data
        var travelData = GenerateTripTravelData(context.Random);

        // Generate trip earnings
        var earnings = GenerateTripEarnings(context.Random, context.Service);

        // Generate trip location data
        var locationData = GenerateTripLocationData(context.Random);

        var tripEntity = new TripEntity
        {
            RowId = rowId,
            Action = ActionTypeEnum.INSERT.GetDescription(),
            Date = context.Date.ToString("yyyy-MM-dd"),
            Number = context.ShiftNumber,
            Service = context.Service,
            Region = context.Region,
            Type = tripType,
            Pickup = tripStart.ToString("T"),
            Dropoff = tripEnd.ToString("T"),
            Duration = tripDuration.ToString(@"hh\:mm\:ss\.fff"),
            Place = locationData.Place,
            StartAddress = locationData.StartAddress,
            Name = locationData.CustomerName,
            EndAddress = locationData.EndAddress,
            Pay = earnings.Pay,
            Tip = earnings.Tip,
            Bonus = earnings.Bonus,
            Cash = earnings.Cash,
            OdometerStart = travelData.OdometerStart,
            OdometerEnd = travelData.OdometerEnd,
            Distance = travelData.Distance,
            Note = $"Demo trip {context.TripNumber}",
            Exclude = context.Random.NextDouble() < 0.05, // 5% chance to exclude
            OrderNumber = context.Random.NextDouble() < 0.1 ? context.Random.Next(100000, 999999).ToString() : string.Empty
        };

        // Occasionally add unit numbers
        if (context.Random.NextDouble() < 0.1)
        {
            var units = new[] { "A", "B", "C", "D", "E", "101", "202", "303", "Apt 5", "Suite 12" };
            tripEntity.EndUnit = units[context.Random.Next(units.Length)];
        }

        return tripEntity;
    }

    /// <summary>
    /// Determines the trip type based on the service.
    /// </summary>
    private static string DetermineTripType(string service)
    {
        return service switch
        {
            "Uber Eats" or "DoorDash" or "Grubhub" => "Pickup",
            "Instacart" or "Shipt" => "Shop",
            _ => "Pickup"
        };
    }

    /// <summary>
    /// Generates trip timing within the shift.
    /// </summary>
    private static (DateTime tripStart, TimeSpan tripDuration, DateTime tripEnd) 
        GenerateTripTiming(Random random, DateTime shiftStart, TimeSpan shiftDuration)
    {
        var tripStart = shiftStart.AddMinutes(random.Next(0, (int)shiftDuration.TotalMinutes));
        var tripDuration = TimeSpan.FromMinutes(random.Next(5, 45));
        var tripEnd = tripStart.Add(tripDuration);

        return (tripStart, tripDuration, tripEnd);
    }

    /// <summary>
    /// Generates trip travel data including odometer and distance.
    /// </summary>
    private static TripTravelData GenerateTripTravelData(Random random)
    {
        // Odometer logic: 30% chance to have odometer values (rare in CSV)
        decimal? odometerStart = random.NextDouble() < 0.3 ? random.Next(10000, 50000) : null;
        decimal? odometerEnd = odometerStart.HasValue ? odometerStart + random.Next(1, 16) : null;
        decimal? distance = null;

        if (odometerStart.HasValue && odometerEnd.HasValue) 
        {
            // 80% chance to use odometer diff as distance, 20% chance to override
            distance = random.NextDouble() < 0.8 
                ? odometerEnd - odometerStart 
                : Math.Round((decimal)random.NextDouble() * 14.5m + 0.6m, 1); // 0.6–15.1 miles (from CSV)
        } 
        else 
        {
            // 90% chance to have distance even if no odometer (most trips have distance in CSV)
            distance = random.NextDouble() < 0.9 ? Math.Round((decimal)random.NextDouble() * 14.5m + 0.6m, 1) : null;
        }

        return new TripTravelData
        {
            OdometerStart = odometerStart,
            OdometerEnd = odometerEnd,
            Distance = distance
        };
    }

    /// <summary>
    /// Generates trip earnings based on service.
    /// </summary>
    private static TripEarningsData GenerateTripEarnings(Random random, string service)
    {
        // Earnings vary by service and type - based on CSV data patterns
        decimal pay;
        decimal? tip = null;
        decimal? bonus = null;
        decimal? cash = null;

        if (service == "DoorDash" || service == "Uber Eats" || service == "Grubhub")
        {
            // CSV shows DoorDash pay: $2.00-$10.75 in $0.25 increments (36 steps from $2.00)
            var basePay = 2.00m + 0.25m * random.Next(0, 36); // $2.00 to $10.75 (0-35 * $0.25)
            pay = Math.Round(basePay, 2);
            
            // Tips: 85% have tips, range $0.50-$16.25, most in $1-$10 range
            if (random.NextDouble() < 0.85)
            {
                tip = random.NextDouble() < 0.7 
                    ? Math.Round((decimal)random.NextDouble() * 9 + 1, 2)      // 70%: $1-$10
                    : Math.Round((decimal)random.NextDouble() * 6.25m + 10, 2); // 15%: $10-$16.25
            }
            
            // Bonus: very rare, usually $1.00 when present
            bonus = random.NextDouble() < 0.08 ? 1.00m : null;
        }
        else if (service == "Instacart" || service == "Shipt")
        {
            // Shop types: CSV shows higher pay $5-$9, tips $0-$10
            pay = Math.Round((decimal)random.NextDouble() * 4 + 5, 2); // $5–$9
            tip = random.NextDouble() < 0.8 ? Math.Round((decimal)random.NextDouble() * 10 + 0.5m, 2) : null;
            bonus = random.NextDouble() < 0.05 ? 1.00m : null;
        }
        else if (service == "Amazon Flex")
        {
            pay = Math.Round((decimal)random.NextDouble() * 20 + 15, 2);
            tip = random.NextDouble() < 0.7 ? Math.Round((decimal)random.NextDouble() * 10, 2) : null;
            bonus = random.NextDouble() < 0.05 ? 1.00m : null;
        }
        else
        {
            // Default: same as DoorDash - $2.00 to $10.75 in $0.25 increments
            pay = Math.Round(2.00m + 0.25m * random.Next(0, 36), 2); // $2.00 to $10.75 (0-35 * $0.25)
            tip = random.NextDouble() < 0.7 ? Math.Round((decimal)random.NextDouble() * 8 + 1, 2) : null;
            bonus = random.NextDouble() < 0.05 ? 1.00m : null;
        }
        
        // Cash tips: very rare, $2-$10 when present (CSV shows ~3% occurrence)
        cash = random.NextDouble() < 0.03 ? Math.Round((decimal)random.NextDouble() * 8 + 2, 2) : null;

        return new TripEarningsData
        {
            Pay = pay,
            Tip = tip,
            Bonus = bonus,
            Cash = cash
        };
    }

    /// <summary>
    /// Generates trip location data including places, addresses, and customer names.
    /// </summary>
    private static TripLocationData GenerateTripLocationData(Random random)
    {
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

        return new TripLocationData
        {
            Place = restaurantPlaces[random.Next(restaurantPlaces.Length)],
            StartAddress = sampleAddresses[random.Next(sampleAddresses.Length)],
            EndAddress = sampleAddresses[random.Next(sampleAddresses.Length)],
            CustomerName = customerName
        };
    }

    /// <summary>
    /// Container for trip travel data.
    /// </summary>
    private record TripTravelData
    {
        public decimal? OdometerStart { get; init; }
        public decimal? OdometerEnd { get; init; }
        public decimal? Distance { get; init; }
    }

    /// <summary>
    /// Container for trip earnings data.
    /// </summary>
    private record TripEarningsData
    {
        public decimal Pay { get; init; }
        public decimal? Tip { get; init; }
        public decimal? Bonus { get; init; }
        public decimal? Cash { get; init; }
    }

    /// <summary>
    /// Container for trip location data.
    /// </summary>
    private record TripLocationData
    {
        public string Place { get; init; } = "";
        public string StartAddress { get; init; } = "";
        public string EndAddress { get; init; } = "";
        public string CustomerName { get; init; } = "";
    }
}

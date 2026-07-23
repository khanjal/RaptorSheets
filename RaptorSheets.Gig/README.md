# RaptorSheets.Gig

[![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) [![Build Status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions)

## Overview

RaptorSheets.Gig is a specialized implementation of RaptorSheets.Core designed for gig work and freelance tracking. It provides pre-configured sheet types, entities, and workflows optimized for managing trips, shifts, expenses, and earnings across multiple gig platforms.

## Table of Contents
1. [Quick Start](#quick-start)
2. [Demo Setup](#demo-setup)
3. [Sheet Types](#sheet-types)
4. [Entities](#entities)
5. [Manager Usage](#manager-usage)
6. [Data Operations](#data-operations)
7. [Advanced Features](#advanced-features)
8. [Examples](#examples)

## Quick Start

### Installation
```bash
dotnet add package RaptorSheets.Gig
```

### Basic Setup
```csharp
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Enums;

// Initialize manager
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create all gig-related sheets
await manager.CreateAllSheets();

// Get your data
var data = await manager.GetAllSheets();
```

### Dependency Injection
```csharp
using RaptorSheets.Gig.Extensions;

// One spreadsheet, bound from configuration
builder.Services.AddRaptorSheetsGig(options =>
{
    options.SpreadsheetId = builder.Configuration["Sheets:SpreadsheetId"];
    options.AccessToken = builder.Configuration["Sheets:AccessToken"];
});
```

`IGoogleSheetManager` is then injectable directly. When each signed-in user has their own
spreadsheet, register without options and create managers per request instead:

```csharp
builder.Services.AddRaptorSheetsGig();

// ... wherever you handle the request:
var manager = factory.Create(userToken, userSpreadsheetId);
```

See [Getting Started](https://github.com/khanjal/RaptorSheets/blob/main/docs/GETTING-STARTED.md#dependency-injection) for details.

## Demo Setup

Generate realistic sample data to explore RaptorSheets.Gig capabilities or set up test environments.

`GenerateDemoData` only *builds* a populated `SheetEntity` in memory — it doesn't touch a
spreadsheet by itself. Create the sheets (if they don't already exist) and write the data with
`ChangeSheetData`, same as any other write:

```csharp
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create the sheets (skip this if they already exist)
await manager.CreateAllSheets();

// Generate 30 days of realistic demo data (Shifts, Trips, Expenses)
var demoData = manager.GenerateDemoData();

// Write it to the spreadsheet
var result = await manager.ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData);

Console.WriteLine("✅ Demo spreadsheet ready!");
```

### Custom Date Ranges and Reproducible Data

```csharp
// Last 90 days
var demoData = manager.GenerateDemoData(
    startDate: DateTime.Today.AddDays(-90),
    endDate: DateTime.Today
);

// Specific quarter
var demoData = manager.GenerateDemoData(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 3, 31)
);

// Deterministic output (same seed → same data, useful for tests)
var demoData = manager.GenerateDemoData(seed: 42);
```

### What Demo Data Includes

The demo system generates realistic gig economy data:

**Shifts** - 1-3 per day (85% of days)
- Services: DoorDash, Uber Eats, Grubhub, Instacart, Amazon Flex, Shipt
- Realistic shift times: 2-8 hours per shift
- Varied regions: Downtown, Suburbs, Airport, University, etc.
- Pay: $20-$220 per shift with tips, bonuses, and cash
- 85% of shifts have trips, 15% are "no-trip" shifts

**Trips** - 2-10 per shift
- Trip types vary by service (Delivery, Pickup)
- Realistic pickup/dropoff times within shift duration
- Pay varies by service ($2-$35 per trip)
- 80% of trips include tips
- Sample addresses, customer names, and places

**Expenses** - 0-2 per day (40% of days)
- Categories: Fuel, Maintenance, Car Wash, Supplies, Parking, Tolls, Phone
- Realistic amounts based on category (Fuel: $20-$60, Maintenance: $30-$130, etc.)

### Complete Demo Creation Example

```csharp
using Google.Apis.Sheets.v4;
using RaptorSheets.Gig.Managers;

public async Task CreateDemoSpreadsheet()
{
    // 1. Create a new Google Spreadsheet
    var sheetsService = new SheetsService(/* credentials */);
    var spreadsheet = await sheetsService.Spreadsheets.Create(new Spreadsheet
    {
        Properties = new SpreadsheetProperties { Title = "RaptorGig Demo" }
    }).ExecuteAsync();
    
    var spreadsheetId = spreadsheet.SpreadsheetId;
    
    // 2. Create the sheets and write generated demo data to them
    var manager = new GoogleSheetManager(credentials, spreadsheetId);
    await manager.CreateAllSheets();
    var demoData = manager.GenerateDemoData();
    var result = await manager.ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData);
    
    // 3. View your demo
    Console.WriteLine($"✅ Demo ready at: https://docs.google.com/spreadsheets/d/{spreadsheetId}");
}
```

### Demo Troubleshooting

**"Unable to save data" Error**
- Ensure spreadsheet exists and has write permissions
- Call `CreateAllSheets()` first if the sheets don't exist yet - `GenerateDemoData()` only builds
  the data in memory, it doesn't create sheets or write anything by itself

**No Data Appearing**
- Verify date range is valid (startDate < endDate)
- Check Google Sheets API permissions
- Review result messages for errors

## Sheet Types

### Core Sheets
Track your primary gig work data:

#### TRIPS
Individual trip/delivery tracking:
- Date, start/end times
- Pickup and dropoff locations
- Customer information
- Pay, tips, and bonuses
- Distance and duration
- Platform/service details

#### SHIFTS
Work session management:
- Shift dates and duration
- Active vs. total time
- Platform/service worked
- Regional information
- Notes and observations

#### EXPENSES
Business expense tracking:
- Expense categories and amounts
- Date and description
- Mileage and gas costs
- Equipment and maintenance

### Summary Helpers
These sheets provide aggregated summaries derived from the `Trips` data. They are generated using query formulas in the mappers and are typically protected/read-only.

- **Deliveries**: Aggregates trips by `Name` and `Address` into `Name | Address | Trips | Pay | Tips | Bonus | Total | Dist | First Trip | Last Trip | Amt/Trip | Amt/Dist`. Implemented by `DeliveryMapper` and exposed via `SheetsConfig.Deliveries`.
- **Locations**: Aggregates trips by `Place` and `Address` into `Place | Address | Trips | Pay | Tips | Bonus | Total | Dist | First Trip | Last Trip | Amt/Trip | Amt/Dist`. Implemented by `LocationMapper` and exposed via `SheetsConfig.Locations`.

These summary sheets are created as part of the normal sheet creation flow (`CreateAllSheets`) when present in the `SheetsConfig.SheetUtilities.GetAllSheetNames()` ordering.

### Auxiliary Sheets
Supporting data for efficiency:

#### ADDRESSES
Frequently visited locations:
- Common pickup/dropoff addresses
- Customer locations
- Restaurant and business addresses

#### NAMES
Customer and contact management:
- Frequent customers
- Business contacts
- Driver/partner names

#### PLACES
Location categorization:
- Restaurants and venues
- Shopping centers
- Common destinations

#### REGIONS
Geographic organization:
- Work areas and zones
- City districts
- Delivery regions

#### SERVICES
Platform and service tracking:
- Gig platforms (Uber, DoorDash, etc.)
- Service types (delivery, rideshare, etc.)
- Special services

#### TYPES
Trip and work categorization:
- Delivery types
- Trip categories
- Work classifications

### Analytics Sheets
Automated reporting and statistics:

#### DAILY
Day-by-day performance:
- Daily earnings summaries
- Trip counts and averages
- Time worked analysis

#### WEEKDAYS
Weekday pattern analysis:
- Monday-Sunday comparisons
- Best performing days
- Time allocation patterns

#### WEEKLY
Weekly performance tracking:
- Week-over-week comparisons
- Weekly goals and targets
- Trend analysis

#### MONTHLY
Monthly summaries:
- Monthly earnings reports
- Expense vs. income analysis
- Growth tracking

#### YEARLY
Annual performance:
- Year-end summaries
- Tax preparation data
- Annual goal tracking

## Entities

### Trip Entity
```csharp
public class TripEntity
{
    public string Date { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Service { get; set; } = "";
    public string Place { get; set; } = "";
    public string Name { get; set; } = "";
    public string StartAddress { get; set; } = "";
    public string EndAddress { get; set; } = "";
    public string EndUnit { get; set; } = "";
    public decimal? Pay { get; set; }
    public decimal? Tip { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Cash { get; set; }
    public decimal? Distance { get; set; }
    public string OrderNumber { get; set; } = "";
    public string Note { get; set; } = "";
    // ... additional properties
}
```

### Shift Entity
```csharp
public class ShiftEntity
{
    public string Date { get; set; } = "";
    public string Start { get; set; } = "";
    public string Finish { get; set; } = "";
    public string Service { get; set; } = "";
    public string Active { get; set; } = "";
    public string Time { get; set; } = "";
    public string Region { get; set; } = "";
    public bool? Omit { get; set; }
    public string Note { get; set; } = "";
    // ... additional properties
}
```

### Expense Entity
```csharp
public class ExpenseEntity
{
    public string Date { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal? Amount { get; set; }
    public decimal? Mileage { get; set; }
    public string Receipt { get; set; } = "";
    public string Note { get; set; } = "";
    // ... additional properties
}
```

### Delivery Entity
```csharp
public class DeliveryEntity
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public int Trips { get; set; }
    public decimal? Pay { get; set; }
    public decimal? Tips { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Total { get; set; }
    public decimal? Distance { get; set; }
    public string FirstTrip { get; set; } = "";
    public string LastTrip { get; set; } = "";
    public decimal AmountPerTrip { get; set; }
    public decimal AmountPerDistance { get; set; }
}
```

### Location Entity
```csharp
public class LocationEntity
{
    public string Place { get; set; } = "";
    public string Address { get; set; } = "";
    public int Trips { get; set; }
    public decimal? Pay { get; set; }
    public decimal? Tips { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Total { get; set; }
    public decimal? Distance { get; set; }
    public string FirstTrip { get; set; } = "";
    public string LastTrip { get; set; } = "";
    public decimal AmountPerTrip { get; set; }
    public decimal AmountPerDistance { get; set; }
}
```


## Manager Usage

> **Architecture note:** `GoogleSheetManager` inherits `GoogleSheetManagerBase<SheetEntity>` from
> `RaptorSheets.Core`. The shared base implements all domain-agnostic behavior — `GetSheets`/
> `GetAllSheets` orchestration (batch read → self-heal missing sheets → unknown-tab detection → map →
> auto-heal missing columns → spreadsheet name), sheet properties, tab names, layouts,
> `InsertMissingColumns`, `GetSpreadsheetInfo`, and `GetBatchData`. The Gig package only adds its
> strongly-typed entities/mappers, its `SheetRegistry<SheetEntity>` (`GigSheetHelpers.Registry`), and
> its Gig-specific write operations (ordered `CreateSheets`, `ChangeSheetData`, `DeleteSheets`, demo
> data). The public API below is unchanged by this — it's just implemented once in Core now.

### GoogleSheetManager Interface
```csharp
public interface IGoogleSheetManager
{
    // CRUD Operations
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateAllSheets();
    Task<SheetEntity> CreateSheets(List<string> sheets);
    Task<SheetEntity> DeleteAllSheets();
    Task<SheetEntity> DeleteSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetAllSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    Task<List<string>> GetAllSheetTabNames();
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);

    // Demo Data Generation - builds data in memory only, see Demo Setup above for writing it
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}
```

### Initialization Options

#### With Access Token
```csharp
var manager = new GoogleSheetManager("your-access-token", "spreadsheet-id");
```

#### With Service Account Credentials
```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = "key-id",
    ["private_key"] = "private-key",
    ["client_email"] = "service@project.iam.gserviceaccount.com",
    ["client_id"] = "client-id"
};

var manager = new GoogleSheetManager(credentials, "spreadsheet-id");
```

## Data Operations

### Creating Sheets
```csharp
// Create all predefined sheets
var result = await manager.CreateAllSheets();

// Create specific sheets
var specificSheets = new List<string> { "Trips", "Shifts", "Expenses" };
var result = await manager.CreateSheets(specificSheets);

// Check for any issues
foreach (var message in result.Messages)
{
    Console.WriteLine($"{message.Level}: {message.Text}");
}
```

### Reading Data
```csharp
// Get all data
var allData = await manager.GetAllSheets();

// Get specific sheet
var tripData = await manager.GetSheet("Trips");
Console.WriteLine($"Found {tripData.Sheets.Trips.Count} trips");

// Get multiple sheets efficiently
var sheets = new List<string> { "Trips", "Shifts" };
var data = await manager.GetSheets(sheets);

// Access the data - row collections live under Sheets, not flat on the entity
foreach (var trip in data.Sheets.Trips)
{
    Console.WriteLine($"Trip: {trip.Date} - ${trip.Pay} + ${trip.Tip}");
}
```

### Updating Data
```csharp
// Prepare your data
var newTrips = new List<TripEntity>
{
    new()
    {
        Date = "2024-01-15",
        StartTime = "09:00",
        EndTime = "09:30", 
        Service = "DoorDash",
        Place = "McDonald's",
        Pay = 8.50m,
        Tip = 3.00m,
        Distance = 2.5m,
        StartAddress = "123 Restaurant St",
        EndAddress = "456 Customer Ave"
    }
};

var sheetEntity = new SheetEntity { Sheets = { Trips = newTrips } };

// Update the sheet
var result = await manager.ChangeSheetData(["Trips"], sheetEntity);

// Handle results
if (result.Messages.Any(m => m.Level == "Error"))
{
    Console.WriteLine("Errors occurred during update:");
    foreach (var error in result.Messages.Where(m => m.Level == "Error"))
    {
        Console.WriteLine($"- {error.Text}");
    }
}
```

### Working with Multiple Data Types
```csharp
// Update multiple sheet types at once - row collections live under Sheets
var sheetEntity = new SheetEntity
{
    Sheets =
    {
        Trips = new List<TripEntity> { /* trip data */ },
        Shifts = new List<ShiftEntity> { /* shift data */ },
        Expenses = new List<ExpenseEntity> { /* expense data */ }
    }
};

var sheetsToUpdate = new List<string> { "Trips", "Shifts", "Expenses" };
var result = await manager.ChangeSheetData(sheetsToUpdate, sheetEntity);
```

## Advanced Features

### Sheet Properties and Validation
```csharp
// Get detailed sheet properties
var properties = await manager.GetAllSheetProperties();

foreach (var prop in properties)
{
    Console.WriteLine($"Sheet: {prop.Name}");
    Console.WriteLine($"Headers: {prop.Attributes["Headers"]}");
    Console.WriteLine($"Max Row: {prop.Attributes["MaxRow"]}");
    Console.WriteLine($"Data Rows: {prop.Attributes["MaxRowValue"]}");
}

// Get properties for specific sheets
var tripProperties = await manager.GetSheetProperties(["Trips"]);
```

### Error Handling and Messages
```csharp
var result = await manager.GetAllSheets();

// Process different message types
foreach (var message in result.Messages)
{
    switch (message.Type)
    {
        case "GET_SHEETS":
            Console.WriteLine($"Sheet operation: {message.Text}");
            break;
        case "CHECK_SHEET":
            Console.WriteLine($"Validation: {message.Text}");
            break;
        case "SAVE_DATA":
            Console.WriteLine($"Save operation: {message.Text}");
            break;
        default:
            Console.WriteLine($"General: {message.Text}");
            break;
    }
}
```

### Header Validation
The library automatically validates sheet headers and provides feedback:

```csharp
// Headers are checked automatically when retrieving data
var data = await manager.GetSheets(["Trips"]);

// Check for header validation messages
var headerIssues = data.Messages
    .Where(m => m.Type == "CHECK_SHEET")
    .ToList();

if (headerIssues.Any())
{
    Console.WriteLine("Header validation issues found:");
    foreach (var issue in headerIssues)
    {
        Console.WriteLine($"- {issue.Text}");
    }
}
```

## Examples

### Daily Workflow Example
```csharp
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Entities;

// Initialize manager
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Record a new shift
var todayShift = new ShiftEntity
{
    Date = DateTime.Today.ToString("yyyy-MM-dd"),
    Start = "09:00",
    Finish = "17:00",
    Service = "Multi-platform",
    Active = "7:30",
    Time = "8:00",
    Region = "Downtown",
    Note = "Busy lunch rush"
};

// Record trips for the day
var todayTrips = new List<TripEntity>
{
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        StartTime = "09:15",
        EndTime = "09:45",
        Service = "UberEats",
        Place = "Starbucks",
        Pay = 6.50m,
        Tip = 2.00m,
        Distance = 1.8m,
        Duration = "30 min"
    },
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        StartTime = "12:00",
        EndTime = "12:25",
        Service = "DoorDash", 
        Place = "Chipotle",
        Pay = 8.75m,
        Tip = 5.00m,
        Distance = 2.2m,
        Duration = "25 min"
    }
};

// Record expenses
var todayExpenses = new List<ExpenseEntity>
{
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        Category = "Fuel",
        Description = "Gas fill-up",
        Amount = 45.00m,
        Mileage = 250
    }
};

// Update all data at once
var sheetEntity = new SheetEntity
{
    Sheets =
    {
        Shifts = [todayShift],
        Trips = todayTrips,
        Expenses = todayExpenses
    }
};

var result = await manager.ChangeSheetData(
    ["Shifts", "Trips", "Expenses"], 
    sheetEntity
);

// Report results
Console.WriteLine($"Updated {todayTrips.Count} trips, 1 shift, {todayExpenses.Count} expenses");
foreach (var message in result.Messages)
{
    Console.WriteLine($"[{message.Level}] {message.Text}");
}
```

### Weekly Report Example
```csharp
// Get weekly data for analysis
var weekData = await manager.GetSheets(["Trips", "Shifts", "Expenses", "Weekly"]);

// Calculate weekly totals
var weekTrips = weekData.Sheets.Trips.Where(t => IsCurrentWeek(t.Date)).ToList();
var totalEarnings = weekTrips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0));
var totalDistance = weekTrips.Sum(t => t.Distance ?? 0);
var totalTrips = weekTrips.Count;

var weekExpenses = weekData.Sheets.Expenses.Where(e => IsCurrentWeek(e.Date)).ToList();
var totalExpenses = weekExpenses.Sum(e => e.Amount ?? 0);

Console.WriteLine($"Week Summary:");
Console.WriteLine($"- Trips: {totalTrips}");
Console.WriteLine($"- Earnings: ${totalEarnings:F2}");
Console.WriteLine($"- Distance: {totalDistance:F1} miles");
Console.WriteLine($"- Expenses: ${totalExpenses:F2}");
Console.WriteLine($"- Net: ${totalEarnings - totalExpenses:F2}");

bool IsCurrentWeek(string dateString)
{
    if (DateTime.TryParse(dateString, out var date))
    {
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        return date >= startOfWeek && date < startOfWeek.AddDays(7);
    }
    return false;
}
```

### Bulk Data Import Example
```csharp
// Import data from CSV or other sources
var csvTrips = ReadTripsFromCsv("trips.csv"); // Your CSV reading logic

var tripEntities = csvTrips.Select(csvTrip => new TripEntity
{
    Date = csvTrip.Date,
    StartTime = csvTrip.StartTime,
    EndTime = csvTrip.EndTime,
    Service = csvTrip.Platform,
    Pay = csvTrip.Earnings,
    Tip = csvTrip.Tips,
    Distance = csvTrip.Miles,
    // ... map other fields
}).ToList();

// Batch import - the library handles efficient API usage
var batchSize = 100; // Process in batches
for (int i = 0; i < tripEntities.Count; i += batchSize)
{
    var batch = tripEntities.Skip(i).Take(batchSize).ToList();
    var batchEntity = new SheetEntity { Sheets = { Trips = batch } };
    
    var result = await manager.ChangeSheetData(["Trips"], batchEntity);
    
    Console.WriteLine($"Imported batch {i/batchSize + 1}: {batch.Count} trips");
    
    // Respect API rate limits
    await Task.Delay(1000);
}
```

## Best Practices

### 1. Efficient Data Operations
```csharp
// Good: Update multiple sheets at once
var result = await manager.ChangeSheetData(
    ["Trips", "Shifts", "Expenses"], 
    sheetEntity
);

// Avoid: Multiple separate calls
await manager.ChangeSheetData(["Trips"], new SheetEntity { Sheets = { Trips = trips } });
await manager.ChangeSheetData(["Shifts"], new SheetEntity { Sheets = { Shifts = shifts } });
await manager.ChangeSheetData(["Expenses"], new SheetEntity { Sheets = { Expenses = expenses } });
```

### 2. Data Validation
```csharp
// Validate data before sending to sheets
bool IsValidTrip(TripEntity trip)
{
    return !string.IsNullOrEmpty(trip.Date) &&
           !string.IsNullOrEmpty(trip.Service) &&
           trip.Pay.HasValue &&
           trip.Pay.Value >= 0;
}

var validTrips = tripList.Where(IsValidTrip).ToList();
if (validTrips.Count != tripList.Count)
{
    Console.WriteLine($"Filtered out {tripList.Count - validTrips.Count} invalid trips");
}
```

### 3. Error Recovery
```csharp
// Implement retry logic for important operations
async Task<SheetEntity> UpdateWithRetry(List<string> sheets, SheetEntity data, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await manager.ChangeSheetData(sheets, data);
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {attempt * 1000}ms...");
            await Task.Delay(attempt * 1000);
        }
    }
    
    throw new InvalidOperationException($"Failed to update sheets after {maxRetries} attempts");
}
```

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify service account email is shared with spreadsheet
   - Check credential format and completeness
   - Ensure Google Sheets API is enabled

2. **Header Validation Warnings**
   - Review expected vs actual headers in messages
   - Recreate sheets if headers are severely mismatched
   - Check for extra spaces or different casing

3. **Rate Limiting**
   - Implement delays between large operations
   - Use batch operations instead of individual calls
   - Monitor API usage in Google Cloud Console

4. **Data Not Appearing**
   - Check for validation errors in Messages
   - Verify data types match entity properties
   - Ensure sheet names match enum descriptions exactly

### Debug Information
```csharp
// Enable detailed logging by examining all messages
var result = await manager.GetAllSheets();

Console.WriteLine("=== Operation Details ===");
foreach (var message in result.Messages)
{
    Console.WriteLine($"[{message.Level}] {message.Type}: {message.Text}");
    Console.WriteLine($"    Time: {DateTimeOffset.FromUnixTimeSeconds(message.Time):yyyy-MM-dd HH:mm:ss}");
}

Console.WriteLine($"\n=== Data Summary ===");
Console.WriteLine($"Trips: {result.Sheets.Trips.Count}");
Console.WriteLine($"Shifts: {result.Sheets.Shifts.Count}");
Console.WriteLine($"Expenses: {result.Sheets.Expenses.Count}");
```

## Support

For Gig-specific issues and questions:
- [Report Issues](https://github.com/khanjal/RaptorSheets/issues) with label `gig`
- [Community Discussions](https://github.com/khanjal/RaptorSheets/discussions)
- [Core Documentation](CORE.md) for underlying functionality
- [Authentication Guide](AUTHENTICATION.md) for setup help

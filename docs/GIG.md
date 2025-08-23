# RaptorSheets.Gig Documentation

## Overview

RaptorSheets.Gig is a specialized implementation of RaptorSheets.Core designed for gig work and freelance tracking. It provides pre-configured sheet types, entities, and workflows optimized for managing trips, shifts, expenses, and earnings across multiple gig platforms.

## Table of Contents
1. [Quick Start](#quick-start)
2. [Sheet Types](#sheet-types)
3. [Entities](#entities)
4. [Manager Usage](#manager-usage)
5. [Data Operations](#data-operations)
6. [Advanced Features](#advanced-features)
7. [Examples](#examples)

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
await manager.CreateSheets();

// Get your data
var data = await manager.GetSheets();
```

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

## Manager Usage

### GoogleSheetManager Interface
```csharp
public interface IGoogleSheetManager
{
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateSheets();
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);
    Task<List<PropertyEntity>> GetSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
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
var result = await manager.CreateSheets();

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
var allData = await manager.GetSheets();

// Get specific sheet
var tripData = await manager.GetSheet("Trips");
Console.WriteLine($"Found {tripData.Trips.Count} trips");

// Get multiple sheets efficiently
var sheets = new List<string> { "Trips", "Shifts" };
var data = await manager.GetSheets(sheets);

// Access the data
foreach (var trip in data.Trips)
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

var sheetEntity = new SheetEntity { Trips = newTrips };

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
// Update multiple sheet types at once
var sheetEntity = new SheetEntity
{
    Trips = new List<TripEntity> { /* trip data */ },
    Shifts = new List<ShiftEntity> { /* shift data */ },
    Expenses = new List<ExpenseEntity> { /* expense data */ }
};

var sheetsToUpdate = new List<string> { "Trips", "Shifts", "Expenses" };
var result = await manager.ChangeSheetData(sheetsToUpdate, sheetEntity);
```

## Advanced Features

### Sheet Properties and Validation
```csharp
// Get detailed sheet properties
var properties = await manager.GetSheetProperties();

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
var result = await manager.GetSheets();

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
    Shifts = [todayShift],
    Trips = todayTrips,
    Expenses = todayExpenses
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
var weekTrips = weekData.Trips.Where(t => IsCurrentWeek(t.Date)).ToList();
var totalEarnings = weekTrips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0));
var totalDistance = weekTrips.Sum(t => t.Distance ?? 0);
var totalTrips = weekTrips.Count;

var weekExpenses = weekData.Expenses.Where(e => IsCurrentWeek(e.Date)).ToList();
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
    var batchEntity = new SheetEntity { Trips = batch };
    
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
await manager.ChangeSheetData(["Trips"], new SheetEntity { Trips = trips });
await manager.ChangeSheetData(["Shifts"], new SheetEntity { Shifts = shifts });
await manager.ChangeSheetData(["Expenses"], new SheetEntity { Expenses = expenses });
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
var result = await manager.GetSheets();

Console.WriteLine("=== Operation Details ===");
foreach (var message in result.Messages)
{
    Console.WriteLine($"[{message.Level}] {message.Type}: {message.Text}");
    Console.WriteLine($"    Time: {DateTimeOffset.FromUnixTimeSeconds(message.Time):yyyy-MM-dd HH:mm:ss}");
}

Console.WriteLine($"\n=== Data Summary ===");
Console.WriteLine($"Trips: {result.Trips?.Count ?? 0}");
Console.WriteLine($"Shifts: {result.Shifts?.Count ?? 0}");
Console.WriteLine($"Expenses: {result.Expenses?.Count ?? 0}");
```

## Support

For Gig-specific issues and questions:
- [Report Issues](https://github.com/khanjal/RaptorSheets/issues) with label `gig`
- [Community Discussions](https://github.com/khanjal/RaptorSheets/discussions)
- [Documentation](../DOCUMENTATION.md) for comprehensive guides
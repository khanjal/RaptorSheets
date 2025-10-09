# RaptorSheets.Gig

[![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) [![Build Status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions)

## Overview

RaptorSheets.Gig is a specialized implementation of RaptorSheets.Core designed for gig work and freelance tracking. It provides pre-configured sheet types, entities, and workflows optimized for managing trips, shifts, expenses, and earnings across multiple gig platforms.

**TypedField System** - Automatic type conversion between Google Sheets and .NET with minimal configuration!

## Table of Contents
1. [Quick Start](#quick-start)
2. [TypedField System](#typedfield-system)
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

### Basic Setup with TypedField System
```csharp
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Repositories;
using RaptorSheets.Gig.Entities;

// Initialize with automatic type conversion
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create all gig-related sheets with automatic formatting
await manager.CreateSheets();

// Get data with automatic type conversion
var data = await manager.GetSheets(); // "$1,234.56" → decimal 1234.56

// Or use repository pattern for type-safe operations
var tripRepository = new TripRepository(sheetService);
var todayTrips = await tripRepository.GetTripsByDateRangeAsync(DateTime.Today, DateTime.Today);
```

## TypedField System

### Automatic Type Conversion

The TypedField system automatically converts between Google Sheets values and .NET types:

```csharp
// Raw Google Sheets data → Strongly typed entities
"$25.50"      → decimal 25.50     (Currency)
"TRUE"        → bool true         (Boolean) 
"(555) 123-4567" → long 5551234567 (PhoneNumber)
"85%"         → decimal 0.85      (Percentage)
"12/25/2023"  → DateTime          (DateTime)
"1,234.56"    → decimal 1234.56   (Number)
```

### Entity Definition with ColumnAttribute

The `ColumnAttribute` system provides clean, single-source configuration:

```csharp
public class TripEntity
{
    public int RowId { get; set; }

    // Header name automatically generates JSON property name "date"
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String)]
    public string Date { get; set; } = "";

    // Automatic currency conversion with default "$#,##0.00" formatting
    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency)]
    public decimal? Tip { get; set; }

    // Custom format when different from default (1 decimal vs 2)
    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, "#,##0.0")]
    public decimal? Distance { get; set; }

    // Override JSON name when needed
    [Column(SheetsConfig.HeaderNames.StartAddress, FieldTypeEnum.String, jsonPropertyName: "fromAddress")]
    public string StartAddress { get; set; } = "";
}
```

### Repository Pattern with Auto-CRUD

```csharp
public class TripRepository : BaseEntityRepository<TripEntity>
{
    public TripRepository(IGoogleSheetService sheetService) 
        : base(sheetService, "Trips", hasHeaderRow: true) { }

    // Business logic with automatic type conversion
    public async Task<List<TripEntity>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var allTrips = await GetAllAsync(); // Automatic conversion
        return allTrips
            .Where(t => !string.IsNullOrEmpty(t.Date) && 
                       DateTime.TryParse(t.Date, out var tripDate) &&
                       tripDate.Date >= startDate.Date && 
                       tripDate.Date <= endDate.Date)
            .ToList();
    }

    public async Task<decimal> GetTotalEarningsAsync(DateTime startDate, DateTime endDate)
    {
        var trips = await GetTripsByDateRangeAsync(startDate, endDate);
        return trips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0));
    }
}
```

### Before vs After Comparison

**Before: Manual Configuration**
```csharp
// Multiple attributes, manual conversions
[JsonPropertyName("pay")]
[ColumnOrder(SheetsConfig.HeaderNames.Pay)]
[TypedField(FieldTypeEnum.Currency, "\"$\"#,##0.00")]
public decimal? Pay { get; set; }

// Manual mapper code
var payValue = HeaderHelpers.GetStringValue("Pay", row, headers);
trip.Pay = decimal.TryParse(payValue.Replace("$", "").Replace(",", ""), out var pay) ? pay : null;
```

**After: ColumnAttribute System**
```csharp
// Single attribute, automatic conversion
[Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
public decimal? Pay { get; set; }

// Automatic CRUD operations
var trips = await repository.GetAllAsync(); // "$25.50" → decimal 25.50
await repository.AddAsync(newTrip);         // decimal 25.50 → "$25.50"
```

## Sheet Types

### Core Sheets
Track your primary gig work data with automatic type conversion:

#### TRIPS
Individual trip/delivery tracking:
- **Date/Time Fields**: Automatic date/time parsing and formatting
- **Location Data**: Address validation and standardization
- **Financial Data**: Automatic currency conversion (`"$25.50"` ↔ `decimal 25.50`)
- **Distance Tracking**: Numeric conversion with custom precision
- **Platform Integration**: Service/platform categorization

#### SHIFTS
Work session management:
- **Time Tracking**: Duration calculations and formatting
- **Platform Monitoring**: Multi-service shift tracking
- **Region Analysis**: Geographic performance data
- **Activity Metrics**: Active vs. total time tracking

#### EXPENSES
Business expense tracking:
- **Automatic Categorization**: Expense type validation
- **Currency Handling**: Multi-currency support with conversion
- **Mileage Tracking**: Integrated distance and cost calculations
- **Receipt Management**: Document reference system

## Entities with TypedField System

### Trip Entity (Enhanced)
```csharp
public class TripEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    // Clean syntax with automatic type handling
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, FieldTypeEnum.String)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, FieldTypeEnum.Integer)]
    public int? Number { get; set; }

    // Currency fields with automatic formatting
    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
    public decimal? Pay { get; set; }    // Auto: "$25.50" ↔ 25.50m

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency)]
    public decimal? Tip { get; set; }    // Auto: "$5.00" ↔ 5.00m

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency)]
    public decimal? Total { get; set; }  // Auto: "$30.50" ↔ 30.50m

    // Custom number formatting (1 decimal place)
    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, "#,##0.0")]
    public decimal? Distance { get; set; } // Auto: "12.5" ↔ 12.5m

    // Address fields with JSON name overrides
    [Column(SheetsConfig.HeaderNames.AddressStart, FieldTypeEnum.String, jsonPropertyName: "startAddress")]
    public string StartAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressEnd, FieldTypeEnum.String, jsonPropertyName: "endAddress")]
    public string EndAddress { get; set; } = "";

    public bool Saved { get; set; }
}
```

### Expense Entity (Enhanced)
```csharp
public class ExpenseEntity
{
    public int RowId { get; set; }

    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.DateTime, "M/d/yyyy")]
    public DateTime? Date { get; set; }   // Auto: "12/25/2023" ↔ DateTime

    [Column(SheetsConfig.HeaderNames.Category, FieldTypeEnum.String)]
    public string Category { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Description, FieldTypeEnum.String)]
    public string Description { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Amount, FieldTypeEnum.Currency)]
    public decimal? Amount { get; set; }  // Auto: "$45.67" ↔ 45.67m

    [Column(SheetsConfig.HeaderNames.Mileage, FieldTypeEnum.Number, "#,##0.0")]
    public decimal? Mileage { get; set; } // Auto: "125.6" ↔ 125.6m

    [Column(SheetsConfig.HeaderNames.Receipt, FieldTypeEnum.String)]
    public string Receipt { get; set; } = "";

    public bool Saved { get; set; }
}
```

## Manager Usage with TypedField System

### Enhanced GoogleSheetManager

```csharp
// Initialize with automatic type conversion
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create sheets with automatic formatting
var result = await manager.CreateSheets();

// TypedField system automatically:
// 1. Applies correct number formats based on FieldTypeEnum
// 2. Sets up data validation rules
// 3. Configures column widths and alignment
// 4. Handles currency, date, and percentage formatting

// Verify setup
foreach (var message in result.Messages)
{
    Console.WriteLine($"[{message.Level}] {message.Text}");
}
```

### Repository-Based Operations

```csharp
// Initialize repositories
var tripRepository = new TripRepository(sheetService);
var expenseRepository = new ExpenseRepository(sheetService);

// Type-safe operations with automatic conversion
var todayTrips = await tripRepository.GetTripsByDateRangeAsync(DateTime.Today, DateTime.Today);
var monthlyExpenses = await expenseRepository.GetExpensesByMonthAsync(DateTime.Today.Month);

// Business logic with converted data
var dailyEarnings = todayTrips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0));
var monthlySpent = monthlyExpenses.Sum(e => e.Amount ?? 0);
var netIncome = dailyEarnings - monthlySpent;

Console.WriteLine($"Today: ${dailyEarnings:F2} earned");
Console.WriteLine($"Month: ${monthlySpent:F2} spent, Net: ${netIncome:F2}");
```

## Data Operations with Auto-Conversion

### Adding Data with Type Safety
```csharp
// Create strongly typed entities
var newTrips = new List<TripEntity>
{
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        Service = "DoorDash",
        Pay = 8.50m,        // Automatically formatted as "$8.50"
        Tip = 3.00m,        // Automatically formatted as "$3.00"
        Distance = 2.5m,    // Automatically formatted as "2.5"
        StartAddress = "123 Restaurant St",
        EndAddress = "456 Customer Ave"
    }
};

// Repository handles all type conversion automatically
foreach (var trip in newTrips)
{
    await tripRepository.AddAsync(trip);
}

// Or bulk operations
await tripRepository.AddRangeAsync(newTrips);
```

### Reading Data with Auto-Conversion
```csharp
// All conversions happen automatically
var allTrips = await tripRepository.GetAllAsync();

foreach (var trip in allTrips)
{
    // All values automatically converted from Google Sheets format
    Console.WriteLine($"Trip on {trip.Date}:");
    Console.WriteLine($"  Service: {trip.Service}");
    Console.WriteLine($"  Pay: ${trip.Pay:F2}");      // Already decimal
    Console.WriteLine($"  Tip: ${trip.Tip:F2}");      // Already decimal
    Console.WriteLine($"  Distance: {trip.Distance:F1} miles"); // Already decimal
    Console.WriteLine($"  Total: ${(trip.Pay ?? 0) + (trip.Tip ?? 0):F2}");
}
```

### Advanced Queries with Type Safety
```csharp
// Repository provides type-safe business logic
public async Task<GigPerformanceReport> GenerateWeeklyReportAsync(DateTime weekStart)
{
    var weekEnd = weekStart.AddDays(7);
    
    // Get data with automatic type conversion
    var weekTrips = await tripRepository.GetTripsByDateRangeAsync(weekStart, weekEnd);
    var weekExpenses = await expenseRepository.GetExpensesByDateRangeAsync(weekStart, weekEnd);
    
    // All arithmetic operations use properly converted types
    return new GigPerformanceReport
    {
        WeekStart = weekStart,
        TotalTrips = weekTrips.Count,
        TotalEarnings = weekTrips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0)),
        TotalExpenses = weekExpenses.Sum(e => e.Amount ?? 0),
        TotalDistance = weekTrips.Sum(t => t.Distance ?? 0),
        AveragePerTrip = weekTrips.Count > 0 
            ? weekTrips.Average(t => (t.Pay ?? 0) + (t.Tip ?? 0)) 
            : 0,
        TopService = weekTrips
            .GroupBy(t => t.Service)
            .OrderByDescending(g => g.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0)))
            .FirstOrDefault()?.Key ?? "None"
    };
}
```

## Advanced Features

### Schema Validation with TypedField System
```csharp
// Automatic schema validation
var tripRepository = new TripRepository(sheetService);
var validation = await tripRepository.ValidateSchemaAsync();

if (!validation.IsValid)
{
    Console.WriteLine("Schema validation failed:");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
    foreach (var warning in validation.Warnings)
    {
        Console.WriteLine($"  Warning: {warning}");
    }
}
else
{
    Console.WriteLine("Schema validation passed - all field types compatible!");
}
```

### Performance Analytics
```csharp
public class GigAnalyticsService
{
    private readonly TripRepository _tripRepository;
    private readonly ExpenseRepository _expenseRepository;
    
    // Automatic type conversion enables complex analytics
    public async Task<Dictionary<string, decimal>> GetServicePerformanceAsync(DateTime startDate, DateTime endDate)
    {
        var trips = await _tripRepository.GetTripsByDateRangeAsync(startDate, endDate);
        
        return trips
            .GroupBy(t => t.Service)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0)) / g.Count() // Average per trip
            );
    }
    
    public async Task<decimal> GetNetProfitAsync(DateTime startDate, DateTime endDate)
    {
        var earnings = await _tripRepository.GetTotalEarningsAsync(startDate, endDate);
        var expenses = await _expenseRepository.GetTotalExpensesAsync(startDate, endDate);
        
        return earnings - expenses; // All values properly converted
    }
}
```

## Examples

### Daily Workflow with TypedField System
```csharp
using RaptorSheets.Gig.Repositories;
using RaptorSheets.Gig.Entities;

// Initialize type-safe repositories
var tripRepository = new TripRepository(sheetService);
var shiftRepository = new ShiftRepository(sheetService);
var expenseRepository = new ExpenseRepository(sheetService);

// Record today's shift
var todayShift = new ShiftEntity
{
    Date = DateTime.Today,
    Start = new TimeSpan(9, 0, 0),   // 9:00 AM
    Finish = new TimeSpan(17, 0, 0), // 5:00 PM
    Service = "Multi-platform",
    Region = "Downtown"
};
await shiftRepository.AddAsync(todayShift);

// Record trips with automatic conversion
var todayTrips = new List<TripEntity>
{
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        Service = "UberEats",
        Pay = 6.50m,      // → "$6.50" in Google Sheets
        Tip = 2.00m,      // → "$2.00" in Google Sheets
        Distance = 1.8m   // → "1.8" in Google Sheets
    },
    new()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd"),
        Service = "DoorDash",
        Pay = 8.75m,      // → "$8.75" in Google Sheets
        Tip = 5.00m,      // → "$5.00" in Google Sheets
        Distance = 2.2m   // → "2.2" in Google Sheets
    }
};

await tripRepository.AddRangeAsync(todayTrips);

// Record expenses
var gasExpense = new ExpenseEntity
{
    Date = DateTime.Today,
    Category = "Fuel",
    Description = "Gas fill-up",
    Amount = 45.00m,    // → "$45.00" in Google Sheets
    Mileage = 250.0m    // → "250.0" in Google Sheets
};
await expenseRepository.AddAsync(gasExpense);

// Generate daily summary
var dailyEarnings = await tripRepository.GetTotalEarningsAsync(DateTime.Today, DateTime.Today);
var dailyExpenses = await expenseRepository.GetTotalExpensesAsync(DateTime.Today, DateTime.Today);

Console.WriteLine($"Daily Summary for {DateTime.Today:yyyy-MM-dd}:");
Console.WriteLine($"  Trips: {todayTrips.Count}");
Console.WriteLine($"  Earnings: ${dailyEarnings:F2}");
Console.WriteLine($"  Expenses: ${dailyExpenses:F2}");
Console.WriteLine($"  Net: ${dailyEarnings - dailyExpenses:F2}");
```

### Performance Analysis with Auto-Conversion
```csharp
public async Task AnalyzeWeeklyPerformanceAsync()
{
    var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
    var endOfWeek = startOfWeek.AddDays(6);
    
    // Get data with automatic type conversion
    var weekTrips = await tripRepository.GetTripsByDateRangeAsync(startOfWeek, endOfWeek);
    var weekExpenses = await expenseRepository.GetExpensesByDateRangeAsync(startOfWeek, endOfWeek);
    
    // All calculations use properly converted numeric types
    var totalEarnings = weekTrips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0));
    var totalExpenses = weekExpenses.Sum(e => e.Amount ?? 0);
    var totalDistance = weekTrips.Sum(t => t.Distance ?? 0);
    var averagePerMile = totalDistance > 0 ? totalEarnings / totalDistance : 0;
    
    // Service performance analysis
    var serviceStats = weekTrips
        .GroupBy(t => t.Service)
        .Select(g => new
        {
            Service = g.Key,
            TripCount = g.Count(),
            TotalEarnings = g.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0)),
            AveragePerTrip = g.Average(t => (t.Pay ?? 0) + (t.Tip ?? 0)),
            TotalDistance = g.Sum(t => t.Distance ?? 0)
        })
        .OrderByDescending(s => s.TotalEarnings)
        .ToList();
    
    Console.WriteLine($"Week of {startOfWeek:MMM dd} - {endOfWeek:MMM dd}");
    Console.WriteLine($"Total: ${totalEarnings:F2} earned, ${totalExpenses:F2} spent");
    Console.WriteLine($"Net Profit: ${totalEarnings - totalExpenses:F2}");
    Console.WriteLine($"Distance: {totalDistance:F1} miles @ ${averagePerMile:F2}/mile");
    Console.WriteLine("\nService Breakdown:");
    
    foreach (var stat in serviceStats)
    {
        Console.WriteLine($"  {stat.Service}: {stat.TripCount} trips, ${stat.TotalEarnings:F2} (${stat.AveragePerTrip:F2}/trip)");
    }
}
```

## Migration from Manual System

### Before: Manual Mappers and Conversions
```csharp
// Old way - lots of manual conversion code
public static List<TripEntity> MapFromRangeData(IList<IList<object>> values, Dictionary<int, string> headers)
{
    var entities = new List<TripEntity>();
    
    foreach (var row in values)
    {
        var entity = new TripEntity();
        
        // Manual conversion for each field
        var payValue = HeaderHelpers.GetStringValue("Pay", row, headers);
        entity.Pay = decimal.TryParse(payValue.Replace("$", "").Replace(",", ""), out var pay) ? pay : null;
        
        var tipValue = HeaderHelpers.GetStringValue("Tips", row, headers);
        entity.Tip = decimal.TryParse(tipValue.Replace("$", "").Replace(",", ""), out var tip) ? tip : null;
        
        var distanceValue = HeaderHelpers.GetStringValue("Distance", row, headers);
        entity.Distance = decimal.TryParse(distanceValue.Replace(",", ""), out var distance) ? distance : null;
        
        // ... dozens more manual conversions
        
        entities.Add(entity);
    }
    
    return entities;
}
```

### After: TypedField System
```csharp
// New way - automatic conversion
public class TripRepository : BaseEntityRepository<TripEntity>
{
    public TripRepository(IGoogleSheetService sheetService) 
        : base(sheetService, "Trips", hasHeaderRow: true) { }
    
    // No manual mapping needed - all automatic!
    // GetAllAsync() handles all conversions based on ColumnAttribute configuration
}

// Usage: One line replaces dozens of manual conversion code
var trips = await tripRepository.GetAllAsync();
```

## Best Practices with TypedField System

### 1. Use Sensible Defaults
```csharp
// Good: Let defaults handle common patterns
[Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
public decimal? Pay { get; set; } // Uses default "$#,##0.00"

// Only specify format when different from default
[Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, "#,##0.0")]
public decimal? Distance { get; set; } // 1 decimal instead of 2
```

### 2. Repository Pattern for Business Logic
```csharp
// Good: Encapsulate business logic in repositories
public class TripRepository : BaseEntityRepository<TripEntity>
{
    public async Task<List<TripEntity>> GetProfitableTripsAsync(decimal minProfit)
    {
        var trips = await GetAllAsync();
        return trips.Where(t => (t.Pay ?? 0) + (t.Tip ?? 0) >= minProfit).ToList();
    }
}

// Avoid: Direct manager usage for complex queries
```

### 3. Schema Validation
```csharp
// Good: Validate schema before operations
var validation = await repository.ValidateSchemaAsync();
if (!validation.IsValid)
{
    // Handle validation errors
    throw new InvalidOperationException($"Schema invalid: {string.Join(", ", validation.Errors)}");
}
```

## Troubleshooting TypedField System

### Common Type Conversion Issues

1. **Currency Conversion Failures**
   ```csharp
   // Problem: Non-standard currency format in sheets
   // Solution: Check format patterns in Google Sheets match FieldTypeEnum.Currency defaults
   
   // Custom currency format if needed
   [Column(SheetsConfig.HeaderNames.EuroAmount, FieldTypeEnum.Currency, "\"€\"#,##0.00")]
   public decimal? EuroAmount { get; set; }
   ```

2. **Date/Time Parsing Issues**
   ```csharp
   // Problem: Date format doesn't match default
   // Solution: Specify custom format pattern
   
   [Column(SheetsConfig.HeaderNames.EuropeanDate, FieldTypeEnum.DateTime, "dd/MM/yyyy")]
   public DateTime? EuropeanDate { get; set; }
   ```

3. **Schema Validation Warnings**
   ```csharp
   // Enable detailed schema validation
   var validation = await repository.ValidateSchemaAsync();
   foreach (var warning in validation.Warnings)
   {
       Console.WriteLine($"Schema Warning: {warning}");
   }
   ```

### Debug Type Conversion
```csharp
// Enable conversion logging for troubleshooting
public async Task<List<TripEntity>> GetTripsWithDebugAsync()
{
    try
    {
        return await repository.GetAllAsync();
    }
    catch (TypedFieldConversionException ex)
    {
        Console.WriteLine($"Conversion failed for field {ex.FieldName}: {ex.Message}");
        Console.WriteLine($"Raw value: '{ex.RawValue}', Target type: {ex.TargetType}");
        throw;
    }
}
```

## Support

For Gig-specific issues and questions:
- 🐞 [Report Issues](https://github.com/khanjal/RaptorSheets/issues) with label `gig`
- 💬 [Community Discussions](https://github.com/khanjal/RaptorSheets/discussions)
- 📖 [Core Documentation](../README.md) for underlying TypedField system functionality
- 🔐 [Authentication Guide](docs/AUTHENTICATION.md) for setup help
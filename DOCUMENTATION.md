# RaptorSheets Complete Guide

## Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Package Selection Guide](#package-selection-guide)
4. [Architecture Deep Dive](#architecture-deep-dive)
5. [Cross-Package Features](#cross-package-features)
6. [Advanced Usage Patterns](#advanced-usage-patterns)
7. [Performance Optimization](#performance-optimization)
8. [Enterprise Features](#enterprise-features)
9. [Migration and Upgrading](#migration-and-upgrading)
10. [Complete API Reference](#complete-api-reference)

## Overview

RaptorSheets is a comprehensive .NET 8 library suite designed to simplify Google Sheets API interactions while providing domain-specific implementations for common use cases. This guide covers the entire ecosystem and advanced usage patterns that span multiple packages.

### Library Architecture Philosophy

RaptorSheets follows a **layered architecture** approach:

```
+-----------------------------------------------------+
|                Your Application                     |
+-----------------------------------------------------+
                            |
+-----------------------------------------------------+
|           Domain-Specific Managers                 |
|     (Gig Manager, Stock Manager, Custom)           |
+-----------------------------------------------------+
                            |
+-----------------------------------------------------+
|              RaptorSheets.Core                     |
|        (Shared Services & Infrastructure)          |
+-----------------------------------------------------+
                            |
+-----------------------------------------------------+
|            Google Sheets API v4                    |
+-----------------------------------------------------+
```

This design enables:
- **Consistent experience** across all implementations
- **Shared infrastructure** for common operations
- **Domain expertise** in specialized packages
- **Easy extensibility** for new use cases

## Getting Started

### Choose Your Package

| If you need... | Install | Documentation |
|----------------|---------|---------------|
| **Gig work tracking** | `RaptorSheets.Gig` | [💼 Gig Guide](docs/GIG.md) |
| **Stock portfolio management** | `RaptorSheets.Stock` | [📈 Stock Guide](docs/STOCK.md) |
| **Custom sheet implementations** | `RaptorSheets.Core` | [🛠️ Core Guide](docs/CORE.md) |

### Universal Setup Steps

1. **Install your chosen package**
   ```bash
   dotnet add package RaptorSheets.Gig    # or Stock, Core
   ```

2. **Set up authentication** - [🔐 Authentication Guide](docs/AUTHENTICATION.md)

3. **Create your first spreadsheet connection**
   ```csharp
   var manager = new GoogleSheetManager(credentials, spreadsheetId);
   ```

4. **Follow package-specific guides** for detailed implementation

## Package Selection Guide

### RaptorSheets.Gig - Freelance & Gig Work
**Best for:** Delivery drivers, rideshare drivers, freelancers, contractors

**Provides:**
- Trip tracking with earnings, tips, distances
- Shift management with time tracking  
- Expense categorization and reporting
- Location and service management
- Automated analytics (daily, weekly, monthly, yearly)

**Key Entities:** `TripEntity`, `ShiftEntity`, `ExpenseEntity`

### RaptorSheets.Stock - Portfolio Management  
**Best for:** Individual investors, portfolio managers, financial tracking

**Provides:**
- Account management across multiple brokerages
- Stock position tracking with cost basis
- Ticker symbol management and categorization
- Performance analytics and reporting

**Key Entities:** `AccountEntity`, `StockEntity`, `TickerEntity`

### RaptorSheets.Core - Custom Implementations
**Best for:** Developers building custom sheet-based applications

**Provides:**
- Low-level Google Sheets API access
- Sheet creation and formatting utilities
- Authentication and error handling
- Extension methods for common operations
- Base infrastructure for custom managers

**Key Classes:** `GoogleSheetService`, `SheetServiceWrapper`, `SheetHelpers`

## Architecture Deep Dive

### Core Components Shared Across All Packages

#### 1. Authentication Layer
All packages share the same authentication mechanisms:

```csharp
// Service Account (Production)
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = "...",
    ["private_key"] = "...",
    ["client_email"] = "...",
    ["client_id"] = "..."
};

// OAuth2 (User-based)
var manager = new GoogleSheetManager("access-token", spreadsheetId);
```

#### 2. Message System
Consistent error handling and operation feedback:

```csharp
public class MessageEntity
{
    public string Level { get; set; }    // Error, Warning, Info
    public string Type { get; set; }     // Operation type
    public string Text { get; set; }     // Human-readable message
    public long Time { get; set; }       // Unix timestamp
}
```

#### 3. Property Management
Sheet metadata and validation:

```csharp
public class PropertyEntity
{
    public string Id { get; set; }       // Sheet ID
    public string Name { get; set; }     // Sheet name
    public Dictionary<string, string> Attributes { get; set; }
}
```

### Package-Specific Layers

Each domain package extends the core with:
- **Specialized entities** for domain data
- **Custom mappers** for data conversion
- **Domain-specific helpers** for common operations
- **Validation rules** appropriate to the domain
- **Pre-configured sheet layouts** optimized for the use case

## Cross-Package Features

### Shared Extension Methods

All packages benefit from core extensions:

```csharp
// String utilities
string result = text.ToTitleCase();
bool isEmpty = value.IsNullOrEmpty();

// Collection operations  
collection.AddRange(items);
headers.UpdateColumns(); // Auto-assigns A, B, C... columns

// Sheet operations
string column = sheet.GetColumn("HeaderName");
string range = sheet.GetRange("HeaderName");
```

### Common Constants and Configurations

Shared across all implementations:

```csharp
public static class GoogleConfig
{
    public const string Range = "A1:ZZ";
    public const string HeaderRange = "1:1";
    public const string RowRange = "A:A";
}

public static class CellFormatPatterns
{
    public const string Currency = "$#,##0.00";
    public const string Percentage = "0.00%";
    public const string Date = "MM/DD/YYYY";
}
```

### Unified Color and Styling System

```csharp
public enum ColorEnum
{
    RED, BLUE, GREEN, YELLOW, ORANGE, PURPLE, CYAN, MAGENTA, BLACK, WHITE
}

// Used consistently across all packages
var sheet = new SheetModel
{
    TabColor = ColorEnum.BLUE,
    CellColor = ColorEnum.LIGHT_GRAY
};
```

## Advanced Usage Patterns

### Multi-Package Applications

You can use multiple RaptorSheets packages in the same application:

```csharp
// Track both gig work and investment portfolio
var gigManager = new RaptorSheets.Gig.Managers.GoogleSheetManager(
    credentials, gigSpreadsheetId);

var stockManager = new RaptorSheets.Stock.Managers.GoogleSheetManager(
    credentials, portfolioSpreadsheetId);

// Coordinate operations
await gigManager.CreateSheets();
await stockManager.CreateSheets();

var gigData = await gigManager.GetSheets();
var stockData = await stockManager.GetSheets();
```

### Custom Package Development

Build your own domain-specific package using Core:

```csharp
// 1. Define your entities
public class CustomEntity
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    // ... domain-specific properties
}

// 2. Create domain-specific manager
public class CustomManager
{
    private readonly GoogleSheetService _service;
    
    public CustomManager(Dictionary<string, string> credentials, string spreadsheetId)
    {
        _service = new GoogleSheetService(credentials, spreadsheetId);
    }
    
    // 3. Implement domain-specific methods
    public async Task<List<CustomEntity>> GetCustomData()
    {
        var data = await _service.GetSheetData("CustomSheet");
        return CustomMapper.MapFromRangeData(data.Values);
    }
}

// 4. Create mappers for your entities
public static class CustomMapper
{
    public static List<CustomEntity> MapFromRangeData(IList<IList<object>> values)
    {
        // Implement mapping logic
    }
    
    public static IList<IList<object>> MapToRangeData(List<CustomEntity> entities)
    {
        // Implement reverse mapping
    }
}
```

### Batch Operations Across Packages

Optimize API usage when working with multiple data types:

```csharp
// Efficient batch operations
var batchRequests = new List<Request>();

// Add requests from different packages
batchRequests.AddRange(GigRequestHelpers.CreateTripRequests(trips));
batchRequests.AddRange(StockRequestHelpers.CreateAccountRequests(accounts));
batchRequests.AddRange(CustomRequestHelpers.CreateCustomRequests(customData));

// Execute all at once
var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = batchRequests };
await coreService.BatchUpdateSpreadsheet(batchRequest);
```

## Performance Optimization

### Request Batching Strategies

```csharp
// Good: Batch multiple operations
var allUpdates = new SheetEntity
{
    Trips = newTrips,
    Shifts = newShifts,
    Expenses = newExpenses
};
await manager.ChangeSheetData(["Trips", "Shifts", "Expenses"], allUpdates);

// Avoid: Individual requests
await manager.ChangeSheetData(["Trips"], new SheetEntity { Trips = newTrips });
await manager.ChangeSheetData(["Shifts"], new SheetEntity { Shifts = newShifts });
await manager.ChangeSheetData(["Expenses"], new SheetEntity { Expenses = newExpenses });
```

### Caching and Rate Limit Management

```csharp
// Cache sheet properties for multiple operations
var properties = await manager.GetSheetProperties();
var cachedProps = properties.ToDictionary(p => p.Name, p => p);

// Use cached data to avoid repeated API calls
foreach (var operation in operations)
{
    var sheetProp = cachedProps[operation.SheetName];
    // Use cached property instead of fetching again
}

// Implement exponential backoff for rate limits
async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxAttempts = 3)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == 429 && attempt < maxAttempts)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            await Task.Delay(delay);
        }
    }
    throw new InvalidOperationException($"Operation failed after {maxAttempts} attempts");
}
```

### Memory Optimization for Large Datasets

```csharp
// Process large datasets in chunks
const int batchSize = 100;
var totalTrips = GetAllTrips(); // Large dataset

for (int i = 0; i < totalTrips.Count; i += batchSize)
{
    var batch = totalTrips.Skip(i).Take(batchSize).ToList();
    var batchEntity = new SheetEntity { Trips = batch };
    
    await manager.ChangeSheetData(["Trips"], batchEntity);
    
    // Optional: Add delay to respect rate limits
    if (i + batchSize < totalTrips.Count)
    {
        await Task.Delay(1000);
    }
    
    // Optional: Clear processed data from memory
    batch.Clear();
}
```

## Enterprise Features

### Multi-Tenant Applications

```csharp
public class MultiTenantSheetManager
{
    private readonly Dictionary<string, IGoogleSheetManager> _managers = new();
    
    public async Task<IGoogleSheetManager> GetManagerForTenant(string tenantId)
    {
        if (!_managers.ContainsKey(tenantId))
        {
            var tenantCredentials = await GetTenantCredentials(tenantId);
            var tenantSpreadsheetId = await GetTenantSpreadsheetId(tenantId);
            
            _managers[tenantId] = new GoogleSheetManager(tenantCredentials, tenantSpreadsheetId);
        }
        
        return _managers[tenantId];
    }
    
    public async Task<Dictionary<string, SheetEntity>> GetAllTenantData()
    {
        var results = new Dictionary<string, SheetEntity>();
        
        var tasks = _managers.Select(async kvp =>
        {
            var data = await kvp.Value.GetSheets();
            return new { TenantId = kvp.Key, Data = data };
        });
        
        var allResults = await Task.WhenAll(tasks);
        
        foreach (var result in allResults)
        {
            results[result.TenantId] = result.Data;
        }
        
        return results;
    }
}
```

### Audit Logging and Compliance

```csharp
public class AuditableSheetManager : IGoogleSheetManager
{
    private readonly IGoogleSheetManager _innerManager;
    private readonly IAuditLogger _auditLogger;
    
    public AuditableSheetManager(IGoogleSheetManager innerManager, IAuditLogger auditLogger)
    {
        _innerManager = innerManager;
        _auditLogger = auditLogger;
    }
    
    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        var auditId = Guid.NewGuid();
        
        await _auditLogger.LogOperationStart(auditId, "ChangeSheetData", new
        {
            Sheets = sheets,
            RecordCounts = new
            {
                Trips = sheetEntity.Trips?.Count ?? 0,
                Shifts = sheetEntity.Shifts?.Count ?? 0,
                Expenses = sheetEntity.Expenses?.Count ?? 0
            }
        });
        
        try
        {
            var result = await _innerManager.ChangeSheetData(sheets, sheetEntity);
            
            await _auditLogger.LogOperationSuccess(auditId, new
            {
                Messages = result.Messages.Count,
                Errors = result.Messages.Count(m => m.Level == "Error")
            });
            
            return result;
        }
        catch (Exception ex)
        {
            await _auditLogger.LogOperationFailure(auditId, ex);
            throw;
        }
    }
    
    // Implement other interface methods with similar audit logging
}
```

### Configuration Management

```csharp
// appsettings.json structure for enterprise deployments
{
  "RaptorSheets": {
    "DefaultTimeout": "00:05:00",
    "RetryAttempts": 3,
    "BatchSize": 100,
    "Environments": {
      "Development": {
        "SpreadsheetIds": {
          "Gig": "dev-gig-spreadsheet-id",
          "Stock": "dev-stock-spreadsheet-id"
        },
        "Credentials": {
          // Development credentials
        }
      },
      "Production": {
        "SpreadsheetIds": {
          "Gig": "prod-gig-spreadsheet-id", 
          "Stock": "prod-stock-spreadsheet-id"
        },
        "CredentialsSource": "AzureKeyVault" // or AWS SecretsManager
      }
    }
  }
}

// Configuration classes
public class RaptorSheetsConfig
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int RetryAttempts { get; set; } = 3;
    public int BatchSize { get; set; } = 100;
    public Dictionary<string, EnvironmentConfig> Environments { get; set; } = new();
}

public class EnvironmentConfig
{
    public Dictionary<string, string> SpreadsheetIds { get; set; } = new();
    public Dictionary<string, string> Credentials { get; set; } = new();
    public string? CredentialsSource { get; set; }
}
```

## Migration and Upgrading

### Upgrading Between Package Versions

```csharp
// Version compatibility check
public static class VersionCompatibility
{
    public static bool IsCompatible(string fromVersion, string toVersion)
    {
        var from = Version.Parse(fromVersion);
        var to = Version.Parse(toVersion);
        
        // Major version changes may require migration
        if (from.Major != to.Major)
        {
            return false;
        }
        
        // Minor/patch versions should be compatible
        return true;
    }
    
    public static async Task<bool> RequiresMigration(IGoogleSheetManager manager)
    {
        // Check if sheet headers match current version expectations
        var properties = await manager.GetSheetProperties();
        
        foreach (var prop in properties)
        {
            var headers = prop.Attributes["Headers"].Split(',');
            if (!ValidateHeaders(prop.Name, headers))
            {
                return true;
            }
        }
        
        return false;
    }
}
```

### Data Migration Utilities

```csharp
public class DataMigrationHelper
{
    public async Task MigrateGigDataV1ToV2(IGoogleSheetManager oldManager, IGoogleSheetManager newManager)
    {
        // 1. Extract data from old format
        var oldData = await oldManager.GetSheets();
        
        // 2. Transform data to new format
        var newTrips = oldData.Trips.Select(oldTrip => new TripEntity
        {
            // Map old fields to new structure
            Date = oldTrip.Date,
            Service = oldTrip.Service,
            Pay = oldTrip.Pay,
            // Handle new fields with defaults or derived values
            PayType = DeterminePayType(oldTrip),
            Category = DetermineTripCategory(oldTrip)
        }).ToList();
        
        // 3. Create new sheets with updated structure
        await newManager.CreateSheets();
        
        // 4. Import transformed data
        var newData = new SheetEntity { Trips = newTrips };
        await newManager.ChangeSheetData(["Trips"], newData);
        
        // 5. Validate migration
        var migratedData = await newManager.GetSheets();
        ValidateMigration(oldData, migratedData);
    }
    
    private PayType DeterminePayType(TripEntity oldTrip)
    {
        // Business logic to determine new field values
        return oldTrip.Cash > 0 ? PayType.Cash : PayType.Digital;
    }
}
```

## Complete API Reference

### Package-Specific Documentation

For detailed API references, see the package-specific guides:

- **[🛠️ Core API Reference](docs/CORE.md)** - Low-level services, extensions, and utilities
- **[💼 Gig API Reference](docs/GIG.md)** - Gig-specific entities, managers, and helpers  
- **[📈 Stock API Reference](docs/STOCK.md)** - Portfolio entities and management operations

### Cross-Package Interfaces

#### IGoogleSheetManager (Common Interface)
```csharp
public interface IGoogleSheetManager
{
    // Data operations
    Task<SheetEntity> GetSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    
    // Management operations
    Task<SheetEntity> CreateSheets();
    Task<List<PropertyEntity>> GetSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
}
```

#### SheetEntity (Universal Data Container)
```csharp
public class SheetEntity
{
    // Gig work data
    public List<TripEntity> Trips { get; set; } = new();
    public List<ShiftEntity> Shifts { get; set; } = new();
    public List<ExpenseEntity> Expenses { get; set; } = new();
    
    // Stock data  
    public List<AccountEntity> Accounts { get; set; } = new();
    public List<StockEntity> Stocks { get; set; } = new();
    public List<TickerEntity> Tickers { get; set; } = new();
    
    // Common data
    public List<SetupEntity> Setup { get; set; } = new();
    public List<MessageEntity> Messages { get; set; } = new();
    public PropertyEntity Properties { get; set; } = new();
}
```

### Google Sheets API Integration

RaptorSheets integrates with these Google APIs:

- **[Google Sheets API v4](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html)** - Primary spreadsheet operations
- **[Google Drive API v3](https://googleapis.dev/dotnet/Google.Apis.Drive.v3/latest/api/Google.Apis.Drive.v3.html)** - File management operations

Key API concepts utilized:
- **BatchUpdateSpreadsheet** - Efficient bulk operations
- **BatchGetValuesByDataFilter** - Optimized data retrieval
- **ValueRange** - Data structure for cell operations
- **SpreadsheetProperties** - Sheet metadata and configuration

## Troubleshooting and Support

### Common Issues Across All Packages

1. **Authentication Problems**
    - See [🔐 Authentication Guide](docs/AUTHENTICATION.md)
   - Verify service account email is shared with spreadsheet
   - Check Google Cloud API enablement

2. **Rate Limiting**
   - Implement exponential backoff (examples above)
   - Use batch operations to reduce API calls
   - Monitor quotas in Google Cloud Console

3. **Data Validation Errors**
   - Check Messages collection in all operation results
   - Verify entity data matches expected formats
   - Use package-specific validation methods

### Getting Help

1. **Package-Specific Issues**: Use the appropriate documentation guide
2. **General Questions**: [GitHub Discussions](https://github.com/khanjal/RaptorSheets/discussions)
3. **Bug Reports**: [GitHub Issues](https://github.com/khanjal/RaptorSheets/issues)
4. **API Questions**: [Google Sheets API Documentation](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html)

### Debug Mode

Enable detailed logging across all packages:

```csharp
public static class DebugHelpers
{
    public static void LogOperationDetails(SheetEntity result, string operation)
    {
        Console.WriteLine($"=== {operation} Results ===");
        Console.WriteLine($"Messages: {result.Messages.Count}");
        
        foreach (var message in result.Messages.GroupBy(m => m.Level))
        {
            Console.WriteLine($"{message.Key}: {message.Count()} messages");
        }
        
        Console.WriteLine("\n=== Data Summary ===");
        Console.WriteLine($"Trips: {result.Trips?.Count ?? 0}");
        Console.WriteLine($"Shifts: {result.Shifts?.Count ?? 0}");
        Console.WriteLine($"Expenses: {result.Expenses?.Count ?? 0}");
        Console.WriteLine($"Accounts: {result.Accounts?.Count ?? 0}");
        Console.WriteLine($"Stocks: {result.Stocks?.Count ?? 0}");
        Console.WriteLine($"Tickers: {result.Tickers?.Count ?? 0}");
    }
}
```

---

This comprehensive guide covers the entire RaptorSheets ecosystem. For implementation details specific to your use case, refer to the individual package documentation listed throughout this guide.
# GenericSheetManager - Flexible Google Sheets Integration

## Overview

The `GenericSheetManager` is a new addition to RaptorSheets.Core that provides flexible, domain-agnostic operations on Google Sheets without requiring specific entity mappings. It's designed for scenarios where you need to work with Google Sheets data in a schema-less manner.

## Key Benefits

- **Schema Flexibility**: Works with any sheet structure without predefined entity classes
- **JSON-First Design**: Naturally serializes to/from JSON for API integrations
- **RaptorSheets Integration**: Uses the same authentication, error handling, and batch operation patterns as other RaptorSheets managers
- **Rich Extension Methods**: Comprehensive helper methods for data analysis, filtering, and transformation
- **Type Safety**: Maintains type safety through generic extension methods while preserving flexibility

## Architecture

### Core Components

1. **GenericSheetManager**: Main manager class implementing `IGenericSheetManager`
2. **GenericSheetResponse**: Response model containing spreadsheet metadata and flexible sheet data
3. **GenericSheetInfo**: Individual sheet information with headers and dictionary-based data
4. **GenericSheetExtensions**: Extension methods for enhanced usability and data manipulation

### Authentication Support

The manager supports both authentication methods used throughout RaptorSheets:

```csharp
// Access token authentication (recommended for client applications)
var manager = new GenericSheetManager(accessToken, spreadsheetId);

// Service account authentication (for server applications)
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["privateKeyId"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY_ID"),
    ["privateKey"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY"),
    ["clientEmail"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_EMAIL"),
    ["clientId"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
};
var manager = new GenericSheetManager(credentials, spreadsheetId);
```

## Core Operations

### Data Retrieval

#### Get Multiple Sheets
```csharp
var sheetNames = new List<string> { "Employees", "Projects", "Tasks" };
var result = await manager.GetSheets(sheetNames);
```

#### Get Single Sheet
```csharp
var result = await manager.GetSheet("Employees");
```

#### Get All Sheets (Discovery)
```csharp
var result = await manager.GetAllSheets();
```

#### Get Sheet Properties Only
```csharp
var result = await manager.GetSheetProperties(sheetNames);
```

### Data Manipulation

#### Update Sheet Data
```csharp
var updateData = new Dictionary<string, List<Dictionary<string, object>>>
{
    ["Employees"] = new()
    {
        new() 
        { 
            ["Name"] = "John Doe", 
            ["Email"] = "john@company.com", 
            ["Department"] = "Engineering"
        }
    }
};
var result = await manager.UpdateSheets(updateData);
```

#### Create New Sheets
```csharp
var sheetConfigs = new Dictionary<string, List<string>>
{
    ["New Employees"] = new() { "ID", "Name", "Email", "Department", "Start Date" }
};
var result = await manager.CreateSheets(sheetConfigs);
```

#### Delete Sheets
```csharp
var sheetsToDelete = new List<string> { "Old Data", "Temporary" };
var result = await manager.DeleteSheets(sheetsToDelete);
```

## Data Model

### GenericSheetResponse Structure

```json
{
  "spreadsheetName": "My Spreadsheet",
  "spreadsheetId": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
  "sheets": {
    "Employees": {
      "sheetId": "123456",
      "headers": ["Name", "Email", "Department"],
      "data": [
        {
          "Name": "John Doe",
          "Email": "john@company.com",
          "Department": "Engineering"
        }
      ],
      "maxRows": 1000,
      "maxColumns": 26,
      "hasData": true
    }
  },
  "messages": [
    {
      "message": "Successfully retrieved 1 sheet(s)",
      "level": "INFO"
    }
  ]
}
```

## Extension Methods

The `GenericSheetExtensions` class provides powerful helper methods:

### JSON Serialization
```csharp
// Pretty-printed JSON
var json = result.ToJson();

// Compact JSON for APIs
var compactJson = result.ToJson(indented: false);
```

### Message Analysis
```csharp
var errors = result.GetErrors();
var warnings = result.GetWarnings();
var successMessages = result.GetSuccessMessages();
```

### Data Filtering
```csharp
// Get only sheets with data
var dataSheets = result.GetSheetsWithData();

// Get empty sheets
var emptySheets = result.GetEmptySheets();

// Find sheets by header columns
var contactSheets = result.FindSheetsByHeaders(new[] { "Email" });

// Find sheets by name pattern
var employeeSheets = result.FindSheetsByName("employee");
```

### Data Analysis
```csharp
// Get unique values across all sheets for a column
var departments = result.GetDistinctColumnValues("Department");

// Get summary statistics
var summary = result.GetSummary();
```

### Type Conversion
```csharp
// Convert sheet data to strongly-typed objects
var employees = result.Sheets["Employees"].ToTypedList<Employee>(row => new Employee
{
    Name = row.GetValueOrDefault("Name", "")?.ToString() ?? "",
    Email = row.GetValueOrDefault("Email", "")?.ToString() ?? ""
});
```

### Data Transformation
```csharp
// Filter data within a sheet
var activeEmployees = employeeSheet.FilterData(row =>
    row.GetValueOrDefault("Status", "")?.ToString() == "Active");

// Group data by column
var byDepartment = employeeSheet.GroupData(row =>
    row.GetValueOrDefault("Department", "Unknown")?.ToString() ?? "Unknown");
```

## Use Cases

### 1. API Integration
Perfect for building REST APIs that expose Google Sheets data:

```csharp
[HttpGet("sheets/{sheetName}")]
public async Task<IActionResult> GetSheetData(string sheetName)
{
    var result = await _genericSheetManager.GetSheet(sheetName);
    
    if (result.HasErrors)
    {
        return BadRequest(result.GetErrors());
    }
    
    return Ok(result.ToJson());
}
```

### 2. Data Discovery
Explore unknown spreadsheet structures:

```csharp
var discovery = await manager.GetAllSheets();
Console.WriteLine(discovery.GetSummary());

// Find all sheets with email data
var contactSheets = discovery.FindSheetsByHeaders(new[] { "email" });
```

### 3. Dynamic Reporting
Build reports from sheets with varying structures:

```csharp
var allData = await manager.GetAllSheets();

foreach (var (sheetName, sheetInfo) in allData.GetSheetsWithData())
{
    Console.WriteLine($"\n=== {sheetName} Report ===");
    Console.WriteLine($"Total Records: {sheetInfo.DataRowCount}");
    
    // Show department breakdown if available
    if (sheetInfo.Headers.Contains("Department"))
    {
        var departments = sheetInfo.GroupData(row => 
            row.GetValueOrDefault("Department", "Unknown")?.ToString() ?? "Unknown");
        
        foreach (var (dept, records) in departments)
        {
            Console.WriteLine($"  {dept}: {records.Count} records");
        }
    }
}
```

### 4. Data Migration
Transfer data between different systems:

```csharp
// Export from Google Sheets
var sourceData = await sourceManager.GetAllSheets();
var exportJson = sourceData.ToJson();

// Transform and import elsewhere
var transformedData = TransformDataForDestination(sourceData);
await destinationSystem.ImportData(transformedData);
```

## Error Handling

The GenericSheetManager follows RaptorSheets error handling patterns:

```csharp
var result = await manager.GetSheets(sheetNames);

if (result.HasErrors)
{
    foreach (var error in result.GetErrors())
    {
        logger.LogError("Sheet operation failed: {Message}", error.Message);
    }
    return;
}

if (result.HasWarnings)
{
    foreach (var warning in result.GetWarnings())
    {
        logger.LogWarning("Sheet operation warning: {Message}", warning.Message);
    }
}

// Process successful result
ProcessSheetData(result);
```

## Performance Considerations

- **Batch Operations**: All operations use Google Sheets batch APIs for optimal performance
- **Lazy Loading**: `GetSheetProperties()` is more efficient when you only need structure information
- **Caching**: Consider caching results for frequently accessed data
- **Rate Limits**: Inherits rate limiting and retry logic from RaptorSheets.Core

## Integration with Domain Managers

The GenericSheetManager complements existing domain-specific managers:

- Use **domain managers** (Gig, Stock) for type-safe, business-logic-heavy operations
- Use **GenericSheetManager** for flexible data exploration, API endpoints, and integration scenarios
- Both can work on the same spreadsheets simultaneously

## Example: Complete Workflow

```csharp
public class SpreadsheetAnalyzer
{
    private readonly GenericSheetManager _manager;
    
    public SpreadsheetAnalyzer(string accessToken, string spreadsheetId)
    {
        _manager = new GenericSheetManager(accessToken, spreadsheetId);
    }
    
    public async Task<SpreadsheetAnalysisResult> AnalyzeSpreadsheet()
    {
        // 1. Discover all sheets
        var discovery = await _manager.GetAllSheets();
        
        if (discovery.HasErrors)
        {
            throw new InvalidOperationException("Failed to access spreadsheet");
        }
        
        // 2. Generate analysis
        var analysis = new SpreadsheetAnalysisResult
        {
            SpreadsheetName = discovery.SpreadsheetName,
            TotalSheets = discovery.Sheets.Count,
            SheetsWithData = discovery.GetSheetsWithData().Count,
            TotalDataRows = discovery.GetSheetsWithData().Sum(s => s.Value.DataRowCount)
        };
        
        // 3. Find contact information
        var contactSheets = discovery.FindSheetsByHeaders(new[] { "email", "phone" });
        analysis.ContactSheets = contactSheets.Keys.ToList();
        
        // 4. Extract unique values for key fields
        analysis.Departments = discovery.GetDistinctColumnValues("Department");
        analysis.Locations = discovery.GetDistinctColumnValues("Location");
        
        // 5. Generate JSON export
        analysis.JsonExport = discovery.ToJson(indented: false);
        
        return analysis;
    }
}
```

## Files Added

1. **`RaptorSheets.Core/Managers/GenericSheetManager.cs`** - Main manager and response models
2. **`RaptorSheets.Core/Extensions/GenericSheetExtensions.cs`** - Extension methods for enhanced functionality  
3. **`RaptorSheets.Core/Examples/GenericSheetManagerExamples.cs`** - Comprehensive usage examples
4. **`RaptorSheets.Core/Enums/MessageTypeEnum.cs`** - Updated with new message types (CREATE_SHEETS, DELETE_SHEETS, etc.)

The GenericSheetManager provides a powerful, flexible way to work with Google Sheets data while maintaining the reliability and patterns established by the RaptorSheets library. It's particularly valuable for API integrations, data exploration, and scenarios where schema flexibility is more important than type safety.
# RaptorSheets.Core Documentation

## Overview

RaptorSheets.Core is the foundational library that provides the essential functionality for interacting with the Google Sheets API. It serves as the base layer for all RaptorSheets implementations and can be used independently for custom Google Sheets integrations.

## Table of Contents
1. [Core Components](#core-components)
2. [Services](#services)
3. [Extensions](#extensions)
4. [Helpers](#helpers)
5. [Models](#models)
6. [Constants](#constants)
7. [Usage Examples](#usage-examples)

## Core Components

### GoogleSheetService
The primary service for Google Sheets API interactions:

```csharp
using RaptorSheets.Core.Services;

var service = new GoogleSheetService(credentials, spreadsheetId);

// Basic operations
var sheetInfo = await service.GetSheetInfo();
var data = await service.GetSheetData("SheetName");
await service.UpdateData(valueRange, "A1:Z100");
```

### SheetServiceWrapper
Low-level wrapper around Google Sheets API v4:

```csharp
using RaptorSheets.Core.Wrappers;

var wrapper = new SheetServiceWrapper(accessToken, spreadsheetId);
var response = await wrapper.BatchUpdateSpreadsheet(batchRequest);
```

## Services

### GoogleSheetService Interface
```csharp
public interface IGoogleSheetService
{
    Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range);
    Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest request);
    Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest request);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range);
    Task<ValueRange?> GetSheetData(string sheet);
    Task<Spreadsheet?> GetSheetInfo(List<string>? ranges);
    Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range);
}
```

### GoogleDriveService
For Google Drive operations:

```csharp
using RaptorSheets.Core.Services;

var driveService = new GoogleDriveService(credentials);
var files = await driveService.ListFiles();
```

## Extensions

### StringExtensions
Utility methods for string manipulation:

```csharp
using RaptorSheets.Core.Extensions;

string text = "hello world";
string result = text.ToTitleCase(); // "Hello World"
bool isEmpty = text.IsNullOrEmpty();
```

### ListExtensions
Extensions for working with sheet headers and data:

```csharp
using RaptorSheets.Core.Extensions;

var headers = new List<SheetCellModel>();
headers.AddColumn(new SheetCellModel { Name = "Column1" });
headers.UpdateColumns(); // Automatically assigns A, B, C... column names
```

### EnumerableExtensions
Collection manipulation utilities:

```csharp
using RaptorSheets.Core.Extensions;

IList<int> collection = [1, 2, 3];
collection.AddRange([4, 5, 6]); // Now contains [1, 2, 3, 4, 5, 6]
```

### ObjectExtensions
Sheet model extensions for easy property access:

```csharp
using RaptorSheets.Core.Extensions;

var sheet = new SheetModel { /* ... */ };
string column = sheet.GetColumn("HeaderName");
string range = sheet.GetRange("HeaderName");
```

## Helpers

### GoogleRequestHelpers
Build Google Sheets API requests:

```csharp
using RaptorSheets.Core.Helpers;

var request = GoogleRequestHelpers.GenerateBatchGetValuesByDataFilterRequest(
    sheets: ["Sheet1", "Sheet2"],
    range: "A1:Z1000"
);
```

### SheetHelpers
Utility methods for sheet operations:

```csharp
using RaptorSheets.Core.Helpers;

// Color management
var color = SheetHelpers.GetColor(ColorEnum.BLUE);

// Column naming
string columnName = SheetHelpers.GetColumnName(26); // Returns "AA"

// Sheet validation
var missingSheets = SheetHelpers.CheckSheets<MySheetEnum>(spreadsheet);
```

### HeaderHelpers
Header validation and processing:

```csharp
using RaptorSheets.Core.Helpers;

var headers = HeaderHelpers.GetHeadersFromCellData(cellData);
var messages = HeaderHelpers.CheckSheetHeaders(actualHeaders, expectedHeaders);
```

### MessageHelpers
Standardized message creation:

```csharp
using RaptorSheets.Core.Helpers;

var errorMsg = MessageHelpers.CreateErrorMessage("Operation failed", MessageTypeEnum.GENERAL);
var infoMsg = MessageHelpers.CreateInfoMessage("Operation succeeded", MessageTypeEnum.SAVE_DATA);
```

## Models

### Core Models
Essential data structures for sheet operations:

```csharp
// Sheet representation
public class SheetModel
{
    public string Name { get; set; }
    public List<SheetCellModel> Headers { get; set; }
    public ColorEnum TabColor { get; set; }
    public bool ProtectSheet { get; set; }
    // ... other properties
}

// Cell representation  
public class SheetCellModel
{
    public string Name { get; set; }
    public string Formula { get; set; }
    public string Column { get; set; }
    public int Index { get; set; }
    // ... other properties
}

// Google API response wrapper
public class GoogleResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### Entity Models

```csharp
// Base property entity
public class PropertyEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Attributes { get; set; } = new();
}

// Message entity for operation feedback
public class MessageEntity
{
    public string Level { get; set; } = "";
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public long Time { get; set; }
}
```

## Constants

### GoogleConfig
Configuration values for Google Sheets operations:

```csharp
public static class GoogleConfig
{
    public const string Range = "A1:ZZ"; // Default range
    public const string HeaderRange = "1:1"; // Header row
    public const string RowRange = "A:A"; // First column
}
```

### Colors
Predefined color values for sheet styling:

```csharp
public static class Colors
{
    public static readonly Color Red = new() { Red = 1.0f, Green = 0.0f, Blue = 0.0f };
    public static readonly Color Blue = new() { Red = 0.0f, Green = 0.0f, Blue = 1.0f };
    // ... other colors
}
```

### CellFormatPatterns
Common formatting patterns:

```csharp
public static class CellFormatPatterns
{
    public const string Currency = "$#,##0.00";
    public const string Percentage = "0.00%";
    public const string Date = "MM/DD/YYYY";
    public const string Time = "HH:MM:SS";
}
```

## Usage Examples

### Basic Sheet Operations

```csharp
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Models.Google;

// Initialize service
var service = new GoogleSheetService(credentials, spreadsheetId);

// Get sheet information
var spreadsheet = await service.GetSheetInfo();
Console.WriteLine($"Spreadsheet: {spreadsheet?.Properties?.Title}");

// Read data from a sheet
var data = await service.GetSheetData("MySheet");
if (data?.Values != null)
{
    foreach (var row in data.Values)
    {
        Console.WriteLine(string.Join(", ", row));
    }
}
```

### Batch Operations

```csharp
using RaptorSheets.Core.Helpers;

// Prepare batch request
var batchRequest = new BatchUpdateSpreadsheetRequest
{
    Requests = new List<Request>
    {
        // Add your requests here
    }
};

// Execute batch update
var response = await service.BatchUpdateSpreadsheet(batchRequest);
if (response != null)
{
    Console.WriteLine($"Updated {response.Replies?.Count} items");
}
```

### Custom Sheet Creation

```csharp
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;

// Define sheet structure
var sheetModel = new SheetModel
{
    Name = "CustomSheet",
    TabColor = ColorEnum.GREEN,
    ProtectSheet = false,
    Headers = new List<SheetCellModel>
    {
        new() { Name = "ID", Format = FormatEnum.NUMBER },
        new() { Name = "Name", Format = FormatEnum.TEXT },
        new() { Name = "Date", Format = FormatEnum.DATE },
        new() { Name = "Amount", Format = FormatEnum.ACCOUNTING }
    }
};

// Generate creation requests
var requests = GoogleRequestHelpers.GenerateSheetCreationRequests(sheetModel);

// Execute creation
await service.BatchUpdateSpreadsheet(new BatchUpdateSpreadsheetRequest 
{ 
    Requests = requests 
});
```

### Error Handling

```csharp
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;

try 
{
    var result = await service.GetSheetData("NonExistentSheet");
}
catch (Exception ex)
{
    var errorMessage = MessageHelpers.CreateErrorMessage(
        $"Failed to retrieve sheet: {ex.Message}", 
        MessageTypeEnum.GET_SHEETS
    );
    
    Console.WriteLine($"[{errorMessage.Level}] {errorMessage.Text}");
}
```

## Authentication

The Core library supports multiple authentication methods. See [AUTHENTICATION.md](AUTHENTICATION.md) for detailed setup instructions.

## Best Practices

### 1. Use Batch Operations
```csharp
// Good: Single batch request
var batchRequest = new BatchUpdateSpreadsheetRequest 
{
    Requests = multipleRequests
};
await service.BatchUpdateSpreadsheet(batchRequest);

// Avoid: Multiple individual requests
foreach (var request in multipleRequests)
{
    await service.BatchUpdateSpreadsheet(new BatchUpdateSpreadsheetRequest 
    { 
        Requests = [request] 
    });
}
```

### 2. Handle API Limits
```csharp
// Implement retry logic for quota exceeded errors
var retryCount = 0;
const int maxRetries = 3;

while (retryCount < maxRetries)
{
    try 
    {
        var result = await service.GetSheetData("MySheet");
        break; // Success
    }
    catch (GoogleApiException ex) when (ex.HttpStatusCode == 429)
    {
        retryCount++;
        await Task.Delay(1000 * retryCount); // Exponential backoff
    }
}
```

### 3. Validate Data
```csharp
using RaptorSheets.Core.Helpers;

// Always validate headers
var actualHeaders = HeaderHelpers.GetHeadersFromCellData(cellData);
var validationMessages = HeaderHelpers.CheckSheetHeaders(actualHeaders, expectedHeaders);

if (validationMessages.Any(m => m.Level == "Error"))
{
    // Handle validation errors
    throw new InvalidOperationException("Sheet headers are invalid");
}
```

## API Reference

For detailed Google Sheets API reference, see:
- [Google Sheets API v4 Documentation](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html)
- [Google Drive API v3 Documentation](https://googleapis.dev/dotnet/Google.Apis.Drive.v3/latest/api/Google.Apis.Drive.v3.html)

## Contributing

When contributing to RaptorSheets.Core:

1. **Maintain Backward Compatibility** - Core library changes affect all implementations
2. **Add Comprehensive Tests** - Both unit and integration tests required
3. **Update Documentation** - Keep this documentation current with changes
4. **Follow Patterns** - Maintain consistency with existing code patterns
5. **Consider Performance** - Core library performance impacts all users

## Support

For Core library specific issues:
- [Report Core Issues](https://github.com/khanjal/RaptorSheets/issues) with label `core`
- [Core Library Discussions](https://github.com/khanjal/RaptorSheets/discussions) for questions
# API Reference

Complete API reference for RaptorSheets libraries.

## Table of Contents
1. [RaptorSheets.Core](#raptorsheetscore)
2. [RaptorSheets.Gig](#raptorsheetsgig)
3. [Common Types](#common-types)
4. [Extension Methods](#extension-methods)

## RaptorSheets.Core

### GoogleSheetService

Primary service for Google Sheets API interactions.

#### Constructor
```csharp
GoogleSheetService(Dictionary<string, string> credentials, string spreadsheetId)
GoogleSheetService(string accessToken, string spreadsheetId)
```

#### Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `GetSheetInfo()` | Get spreadsheet metadata | `Task<Spreadsheet>` |
| `GetSheetData(string sheetName)` | Get data from specific sheet | `Task<ValueRange>` |
| `UpdateData(ValueRange valueRange, string range)` | Update sheet data | `Task<UpdateValuesResponse>` |
| `BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest request)` | Execute batch operations | `Task<BatchUpdateSpreadsheetResponse>` |
| `CreateSheet(string sheetName)` | Create new sheet | `Task<Sheet>` |
| `DeleteSheet(int sheetId)` | Delete sheet | `Task` |

### SheetServiceWrapper

Low-level wrapper around Google Sheets API.

#### Methods

| Method | Description |
|--------|-------------|
| `GetSpreadsheet(string spreadsheetId)` | Get spreadsheet properties |
| `GetValues(string spreadsheetId, string range)` | Get cell values |
| `UpdateValues(string spreadsheetId, string range, ValueRange valueRange)` | Update cell values |
| `BatchUpdate(string spreadsheetId, BatchUpdateSpreadsheetRequest request)` | Batch operations |

### SheetHelpers

Utility methods for sheet operations.

#### Static Methods

| Method | Description |
|--------|-------------|
| `GetColumn(string headerName, List<SheetCellModel> headers)` | Get column letter for header |
| `GetRange(string headerName, List<SheetCellModel> headers)` | Get range for header column |
| `UpdateColumns(List<SheetCellModel> headers)` | Assign column letters A, B, C... |

## RaptorSheets.Gig

### GoogleSheetManager

High-level manager for gig work tracking.

#### Constructor
```csharp
GoogleSheetManager(Dictionary<string, string> credentials, string spreadsheetId)
GoogleSheetManager(string accessToken, string spreadsheetId)
```

#### Core Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `CreateSheets()` | Create all gig tracking sheets | `Task<SheetEntity>` |
| `GetSheets()` | Get all sheet data | `Task<SheetEntity>` |
| `GetSheets(List<SheetEnum> sheets)` | Get specific sheets | `Task<SheetEntity>` |
| `GetSheetProperties()` | Get sheet metadata | `Task<List<PropertyEntity>>` |

#### Data Management Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `AddTrips(List<TripsEntity> trips)` | Add trip records | `Task<SheetEntity>` |
| `AddShifts(List<ShiftsEntity> shifts)` | Add shift records | `Task<SheetEntity>` |
| `AddExpenses(List<ExpensesEntity> expenses)` | Add expense records | `Task<SheetEntity>` |
| `ChangeSheetData(List<string> sheets, SheetEntity data)` | Bulk update multiple sheets | `Task<SheetEntity>` |

### Entity Classes

#### TripsEntity
```csharp
public class TripsEntity
{
    public int RowId { get; set; }
    public string Date { get; set; }
    public string Service { get; set; }
    public string Type { get; set; }
    public decimal Pay { get; set; }
    public decimal? Tips { get; set; }
    public decimal? Bonus { get; set; }
    public string AddressStart { get; set; }
    public string AddressEnd { get; set; }
    public string NameStart { get; set; }
    public string NameEnd { get; set; }
    // ... additional properties
}
```

#### ShiftsEntity
```csharp
public class ShiftsEntity
{
    public int RowId { get; set; }
    public string Date { get; set; }
    public string Service { get; set; }
    public decimal? Pay { get; set; }
    public decimal? Tips { get; set; }
    public decimal? Bonus { get; set; }
    public string TimeStart { get; set; }
    public string TimeEnd { get; set; }
    // ... additional properties
}
```

#### ExpensesEntity
```csharp
public class ExpensesEntity
{
    public int RowId { get; set; }
    public string Date { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    // ... additional properties
}
```

### Enums

#### SheetEnum
```csharp
public enum SheetEnum
{
    Trips,
    Shifts,
    Expenses,
    Addresses,
    Names,
    Places,
    Regions,
    Services,
    Types,
    Daily,
    Weekdays,
    Weekly,
    Monthly,
    Yearly,
    Setup
}
```

## Common Types

### SheetEntity
Main data container for all sheet operations.

```csharp
public class SheetEntity
{
    public List<TripsEntity> Trips { get; set; } = new();
    public List<ShiftsEntity> Shifts { get; set; } = new();
    public List<ExpensesEntity> Expenses { get; set; } = new();
    public List<AddressesEntity> Addresses { get; set; } = new();
    // ... other sheet types
    public List<MessageEntity> Messages { get; set; } = new();
    public List<PropertyEntity> Properties { get; set; } = new();
}
```

### MessageEntity
Operation feedback and error reporting.

```csharp
public class MessageEntity
{
    public string Level { get; set; }    // "Error", "Warning", "Info"
    public string Type { get; set; }     // Operation type
    public string Text { get; set; }     // Human-readable message
    public long Time { get; set; }       // Unix timestamp
}
```

### PropertyEntity
Sheet metadata and configuration.

```csharp
public class PropertyEntity
{
    public string Id { get; set; }       // Sheet ID
    public string Name { get; set; }     // Sheet name
    public Dictionary<string, string> Attributes { get; set; } = new();
}
```

### SheetModel
Sheet structure definition (Core).

```csharp
public class SheetModel
{
    public string Name { get; set; }
    public ColorEnum TabColor { get; set; }
    public ColorEnum CellColor { get; set; }
    public int FreezeRowCount { get; set; }
    public int FreezeColumnCount { get; set; }
    public bool ProtectSheet { get; set; }
    public List<SheetCellModel> Headers { get; set; } = new();
}
```

### SheetCellModel
Individual cell definition (Core).

```csharp
public class SheetCellModel
{
    public string Name { get; set; }
    public string Column { get; set; }
    public FormatEnum Format { get; set; }
    public string Formula { get; set; }
    public bool Protect { get; set; }
    public List<string> DropdownValues { get; set; } = new();
}
```

## Extension Methods

### String Extensions
```csharp
string.ToTitleCase()              // Convert to title case
string.IsNullOrEmpty()            // Check if null or empty
string.ToSafeString()             // Convert to safe string (handles nulls)
```

### Collection Extensions
```csharp
List<T>.AddRange(IEnumerable<T>)  // Add multiple items
List<T>.IsNullOrEmpty()           // Check if null or empty
```

### Sheet Extensions
```csharp
List<SheetCellModel>.UpdateColumns()                    // Assign A, B, C... columns
List<SheetCellModel>.GetColumn(string headerName)       // Get column letter
List<SheetCellModel>.GetRange(string headerName)        // Get range string
```

### Header Extensions
```csharp
IList<object>.GetStringValue(string column, Dictionary<int, string> headers)
IList<object>.GetIntValue(string column, Dictionary<int, string> headers)  
IList<object>.GetDecimalValue(string column, Dictionary<int, string> headers)
IList<object>.GetDateValue(string column, Dictionary<int, string> headers)
```

## Enums Reference

### ColorEnum
```csharp
public enum ColorEnum
{
    RED, BLUE, GREEN, YELLOW, ORANGE, PURPLE, CYAN, MAGENTA, 
    BLACK, WHITE, LIGHT_RED, LIGHT_BLUE, LIGHT_GREEN, 
    LIGHT_YELLOW, LIGHT_ORANGE, LIGHT_PURPLE, LIGHT_CYAN, 
    LIGHT_MAGENTA, LIGHT_GRAY, DARK_GRAY
}
```

### FormatEnum
```csharp
public enum FormatEnum
{
    TEXT, NUMBER, CURRENCY, PERCENTAGE, DATE, TIME, 
    CURRENCY_ROUNDED, ACCOUNTING, SCIENTIFIC
}
```

## Constants

### GoogleConfig
```csharp
public static class GoogleConfig
{
    public const string Range = "A1:ZZ";
    public const string HeaderRange = "1:1";
    public const string RowRange = "A:A";
}
```

### CellFormatPatterns
```csharp
public static class CellFormatPatterns
{
    public const string Currency = "$#,##0.00";
    public const string CurrencyRounded = "$#,##0";
    public const string Percentage = "0.00%";
    public const string Date = "MM/DD/YYYY";
    public const string Time = "HH:MM:SS";
}
```

## Error Handling

All methods return meaningful error messages through the `MessageEntity` system:

```csharp
var result = await manager.AddTrips(trips);

foreach (var message in result.Messages)
{
    Console.WriteLine($"{message.Level}: {message.Text}");
}

// Check for errors
bool hasErrors = result.Messages.Any(m => m.Level == "Error");
```

## Authentication Examples

### Service Account
```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = "key-id",
    ["private_key"] = "-----BEGIN PRIVATE KEY-----\n...",
    ["client_email"] = "service@project.iam.gserviceaccount.com",
    ["client_id"] = "123456789"
};
```

### OAuth2 Access Token
```csharp
string accessToken = "ya29.a0AfH6SMC...";
var manager = new GoogleSheetManager(accessToken, spreadsheetId);
```

## Rate Limits and Quotas

- **Read/Write requests**: 100 per 100 seconds per user
- **Daily requests**: 50,000 per day
- All RaptorSheets methods automatically handle retries and rate limiting
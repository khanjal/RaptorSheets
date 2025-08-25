# RaptorSheets Copilot Instructions

## Project Overview
RaptorSheets is a .NET 8 library suite for Google Sheets API integration with domain-specific packages for gig work tracking, stock portfolio management, and a shared Core library. The architecture follows a layered approach: Domain Packages → Core Services → API Wrappers → Google Sheets API.

## Architecture Patterns

### Layered Architecture
- **Domain Managers** (`RaptorSheets.Gig.Managers`, `RaptorSheets.Stock.Managers`): High-level business operations
- **Core Services** (`RaptorSheets.Core.Services.GoogleSheetService`): Unified Google API abstraction
- **API Wrappers** (`RaptorSheets.Core.Wrappers.SheetServiceWrapper`): Direct Google API communication
- **Mappers**: Static classes converting between entities and Google API data structures

### Entity-Mapper Pattern
All entities follow this pattern:
```csharp
// Entity definition
public class TripEntity : AmountEntity { ... }

// Corresponding mapper
public static class TripMapper
{
    public static List<TripEntity> MapFromRangeData(IList<IList<object>> values) { ... }
    public static SheetModel GetSheet() { ... } // Sheet configuration
}
```

### Batch Operations First
Always use batch operations for Google API calls:
- `BatchUpdateSpreadsheetRequest` for sheet modifications
- `BatchGetValuesByDataFilterRequest` for data retrieval
- Single requests are only for individual operations

## Key Conventions

### Authentication
Two authentication patterns supported:
- **Access Token**: `string accessToken` (preferred for external applications, follows Google auth standards)
- **Service Account**: `Dictionary<string, string>` credentials (primarily for testing and CI/CD)

### Error Handling
All operations return `MessageEntity` collections with structured feedback:
```csharp
// Good: Structured error handling
var result = await manager.GetSheets();
if (result.Messages.Any(m => m.Level == "ERROR")) { ... }
```

### Header Management
Headers use `HeaderEnum` with `GetDescription()` for column mapping:
```csharp
var value = HeaderHelpers.GetStringValue(HeaderEnum.SERVICE.GetDescription(), row, headers);
```

### Action Types
Use `ActionTypeEnum` for CRUD operations on entities:
- `ActionTypeEnum.ADD`, `UPDATE`, `DELETE` stored in entity `Action` property
- Mappers check action type to generate appropriate Google API requests

## Development Workflows

### Build & Test
```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### Integration Tests
- Use real Google Sheets with test credentials stored in GitHub Secrets
- Test categories: `[Category("Unit Tests")]`, `[Category("Integration Tests")]`
- Helper classes in `Tests\Data\Helpers` for generating test data

### Package Structure
Each domain package follows this structure:
```
RaptorSheets.{Domain}/
├── Managers/GoogleSheetManager.cs    # Main API surface
├── Entities/                         # Domain models
├── Mappers/                         # Data transformation
├── Helpers/                         # Domain-specific utilities
├── Enums/SheetEnum.cs              # Sheet type definitions
└── Constants/                      # Domain constants
```

## Critical Implementation Details

### Sheet Creation Pattern
Sheets are created with predefined layouts using `GenerateSheetsHelpers`:
```csharp
var batchRequest = GenerateSheetsHelpers.Generate(sheetNames);
await _googleSheetService.BatchUpdateSpreadsheet(batchRequest);
```

### Data Validation & Protection
- Headers are automatically protected with `GenerateProtectedRangeForHeaderOrSheet`
- Data validation uses `ValidationEnum` for dropdowns and constraints
- Sheet styling includes alternating row colors and tab colors

### Message System
All operations return structured messages using `MessageHelpers`:
```csharp
MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {sheetList}", MessageTypeEnum.GET_SHEETS)
```

### Extension Method Usage
Heavily uses extension methods for:
- Enum descriptions: `SheetEnum.TRIPS.GetDescription()`
- String conversions: `"2023-12-25".ToSerialDate()`
- Collection helpers: `value.AddItems(count)`

## Testing Patterns

### Unit Tests
- Mappers: Test data transformation in both directions
- Helpers: Test utility functions with edge cases
- Use `Theory`/`InlineData` for parameterized tests

### Integration Tests
- End-to-end workflows in `CoreIntegrationTests`
- Real Google API operations (when credentials available)
- Test helper classes generate realistic test data with known patterns

### Test Data Generation
Use `TestGigHelpers.GenerateSelectiveDeletionTestData()` patterns for complex scenarios.

## Common Gotchas

1. **Sheet Names**: Use enum descriptions (`SheetEnum.TRIPS.GetDescription()`) not enum names
2. **Range Formatting**: Always use `GoogleConfig.Range` for data ranges, `GoogleConfig.HeaderRange` for headers
3. **Null Handling**: Check for null Google API responses before processing
4. **Row IDs**: Entity `RowId` maps to Google Sheets row numbers (1-indexed)
5. **Missing Sheets**: Use `HandleMissingSheets()` pattern to auto-create missing sheets

## When Adding New Features

1. **New Entity**: Create entity class, mapper with `MapFromRangeData` and `GetSheet` methods
2. **New Sheet Type**: Add to appropriate `SheetEnum`, update manager switch statements  
3. **New Domain**: Follow existing package structure, implement `IGoogleSheetManager` interface
4. **API Changes**: Update both directions - entity to Google API and Google API to entity

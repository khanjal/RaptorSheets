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

Follow the standardized pattern for sheet configuration:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.YourSheet;
    sheet.Headers.UpdateColumns(); // Essential: Sets proper column indexes
    
    sheet.Headers.ForEach(header => {
        var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
        switch (headerEnum) {
            case HeaderEnum.DATE:
                header.Format = FormatEnum.DATE;
                break;
            // ... configure other headers
        }
    });
    
    return sheet;
}
```

### Google Formula Management
Use the centralized formula system for consistency and maintainability:

**New Approach (Recommended):**
```csharp
// Use GoogleFormulaBuilder for generic Google Sheets patterns
header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
    keyRange, 
    HeaderEnum.PAY.GetDescription(), 
    lookupRange, 
    sumRange
);

// Use domain-specific builders for business logic (e.g., GigFormulaBuilder for gig formulas)
header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
    keyRange, 
    HeaderEnum.TOTAL.GetDescription(), 
    payRange, 
    tipsRange, 
    bonusRange
);

// For complex custom formulas
header.Formula = GoogleFormulaBuilder.BuildCustomFormula(
    GoogleFormulas.ArrayFormulaBase,
    ("{keyRange}", keyRange),
    ("{header}", headerName),
    ("{formula}", customBusinessLogic)
);
```

**Legacy Approach (Still Supported):**
```csharp
// ArrayFormulaHelpers methods still work but are marked obsolete
header.Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, header, range, sumRange);
```

**Available Formula Systems:**

**Generic Templates (`GoogleFormulas`):**
- `GoogleFormulas.ArrayFormulaBase` - Foundation for all ARRAYFORMULA constructs
- `GoogleFormulas.SumIfAggregation` - SUMIF aggregation patterns
- `GoogleFormulas.SafeDivisionFormula` - Zero-safe division
- `GoogleFormulas.SortedVLookup` - Generic lookup patterns
- `GoogleFormulas.WeekdayNumber` - Generic date calculations

**Gig-Specific Templates (`GigFormulas`):**
- `GigFormulas.TotalIncomeFormula` - Pay + Tips + Bonus calculation
- `GigFormulas.AmountPerTripFormula` - Revenue per trip analysis
- `GigFormulas.WeekNumberWithYear` - Gig-specific date formatting
- `GigFormulas.CurrentAmountLookup` - Weekday analysis patterns
- `GigFormulas.MultipleFieldVisitLookup` - Address tracking logic

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
- Integration tests can provide better coverage than unit tests for some scenarios (e.g., testing with real Google API responses vs. demo data)

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

Available message types include:
- `CHECK_SHEET` - Header validation errors (tolerated during refactoring)
- `API_ERROR` - Google API communication issues
- `AUTHENTICATION` - Auth-related problems
- `VALIDATION` - Data validation errors

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
- End-to-end workflows in `GoogleSheetIntegrationWorkflow`
- Real Google API operations (when credentials available)
- Test helper classes generate realistic test data with known patterns
- Can replace unit tests when they provide better coverage (e.g., testing with real spreadsheet data vs. demo JSON)

### Test Data Generation
Use `TestGigHelpers.GenerateSelectiveDeletionTestData()` patterns for complex scenarios.

## Common Gotchas

1. **Sheet Names**: Use enum descriptions (`SheetEnum.TRIPS.GetDescription()`) not enum names
2. **Range Formatting**: Always use `GoogleConfig.Range` for data ranges, `GoogleConfig.HeaderRange` for headers
3. **Null Handling**: Check for null Google API responses before processing
4. **Row IDs**: Entity `RowId` maps to Google Sheets row numbers (1-indexed)
5. **Missing Sheets**: Use `HandleMissingSheets()` pattern to auto-create missing sheets
6. **Header Indexing**: Always call `sheet.Headers.UpdateColumns()` after modifying Headers collection
7. **Formula Duplication**: Use `GoogleFormulaBuilder` to avoid repeating complex formula strings across mappers

## When Adding New Features

### New Mapper Implementation
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.YourSheet; // Start with config
    sheet.Headers.UpdateColumns();      // Essential: Set column indexes
    
    // Use helper methods or configure headers individually
    sheet.Headers.ForEach(header => {
        var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
        switch (headerEnum) {
            case HeaderEnum.TOTAL:
                header.Formula = GoogleFormulaBuilder.BuildArrayFormulaTotal(
                    keyRange, header.Name, payRange, tipsRange, bonusRange
                );
                header.Format = FormatEnum.ACCOUNTING;
                break;
        }
    });
    
    return sheet;
}
```

### New Google Formulas
When adding complex formulas:
1. **Add template to `GoogleFormulas`** with placeholder tokens
2. **Create builder method in `GoogleFormulaBuilder`** for type safety
3. **Use builder in mappers** instead of string concatenation
4. **Add unit tests** for individual formula templates

### Migration from Legacy Formulas
When encountering obsolete warnings:
```csharp
// Old approach - will show obsolete warnings
var formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, header, range, sumRange);

// New approach - preferred
var formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, range, sumRange);
```

1. **New Entity**: Create entity class, mapper with `MapFromRangeData` and `GetSheet` methods
2. **New Sheet Type**: Add to appropriate `SheetEnum`, update manager switch statements  
3. **New Domain**: Follow existing package structure, implement `IGoogleSheetManager` interface
4. **API Changes**: Update both directions - entity to Google API and Google API to entity

## Formula Management Best Practices

### Proper Separation of Concerns

**Use Generic Builders for Common Patterns:**
```csharp
// Generic Google Sheets patterns (any domain can use)
header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
    keyRange, headerName, lookupRange, sumRange
);
header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
    keyRange, headerName, lookupRange
);
header.Formula = GoogleFormulaBuilder.BuildSafeDivision(numerator, denominator);
```

**Use Domain Builders for Business Logic:**
```csharp
// Gig-specific business logic formulas
header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
    keyRange, headerName, payRange, tipsRange, bonusRange
);
header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(
    keyRange, headerName, totalRange, tripsRange
);
header.Formula = GigFormulaBuilder.BuildArrayFormulaCurrentAmount(
    keyRange, headerName, dayRange, dailySheet, dateColumn, totalColumn, totalIndex
);
```

### Formula Organization

**Core Package (`GoogleFormulas` / `GoogleFormulaBuilder`):**
- Generic ARRAYFORMULA patterns
- Common aggregation functions (SUMIF, COUNTIF, VLOOKUP)
- Generic date/time calculations
- Safe division and error handling patterns

**Domain Packages (`GigFormulas` / `GigFormulaBuilder`, etc.):**
- Business logic specific to that domain
- Complex calculations unique to the domain
- Domain-specific date/time formatting
- Multi-field lookups with domain context

### Migration Pattern

**Don't Do This (Avoid):**
```csharp
// Hard to maintain, test, or debug
header.Formula = $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\",true,SUMIF({range},{keyRange},{sumRange})))";
```

**Do This (Recommended):**
```csharp
// For generic patterns - use Core builders
header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, range, sumRange);

// For domain-specific logic - use domain builders  
header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, header, payRange, tipsRange, bonusRange);
```

### Legacy Support
```csharp
// Legacy ArrayFormulaHelpers still work but show obsolete warnings
header.Formula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, header, range, sumRange);
// → Suggests using GoogleFormulaBuilder.BuildArrayFormulaSumIf instead

header.Formula = ArrayFormulaHelpers.ArrayFormulaTotal(keyRange, header, payRange, tipsRange, bonusRange);
// → Suggests using GigFormulaBuilder.BuildArrayFormulaTotal instead
```

The separation ensures that generic Google Sheets functionality stays in Core while domain-specific business logic stays in the appropriate domain packages, making the code more maintainable and testable.

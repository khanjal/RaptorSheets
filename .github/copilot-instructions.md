# Google Sheets Management System Documentation

## Overview

The RaptorSheets system manages Google Sheets through a layered architecture that separates concerns between configuration, data modeling, and business logic. The system handles complex spreadsheets with automated formulas, cross-sheet references, and **optional** column and sheet ordering.

## AI Contribution Guidelines

- **Put shared logic in Core**: Entities, helpers, models, enums, request/response builders, validation helpers, and constants that can apply across domains belong in `RaptorSheets.Core`. Domain projects (`RaptorSheets.Gig`, `RaptorSheets.Stock`, etc.) should consume these shared pieces instead of duplicating them.
- **Default shared types to Core**: If a type is or could be used across domains (e.g., formatting options, request models, common entities), add it to `RaptorSheets.Core` and reference it—avoid parallel domain copies.
- **Keep domain-specific code in its project**: Only domain-specific managers, mappers, and configuration live in the domain project. Prefer referencing Core types instead of redefining them.
- **Reuse centralized constants**: Always use the shared constants (sheet names, headers, formats) from Core/SheetsConfig to avoid drift.
- **Prefer entity-driven generation**: Use `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` and constants-based sheet ordering; avoid manual header or tab lists unless there is a justified exception.
- **Leverage existing helpers**: Before adding new helpers, check `Core.Helpers` (e.g., `GoogleRequestHelpers`, `SheetHelpers`, `MapperHelper`) and extend them rather than creating parallel utilities.
- **Extend GoogleRequestHelpers in Core**: When adding new Google Sheets API requests, put them in `RaptorSheets.Core/Helpers/GoogleRequestHelpers.cs` so all domains share the same request builders.
- **Use shared formatting models**: Reuse `FormattingOptionsEntity` (in Core) for any formatting/metadata operations; orchestration belongs in domain managers (e.g., `GoogleSheetManager`) but models/helpers stay in Core.
- **Keep constants single-source**: Reference sheet/header/format constants from `SheetsConfig` (or domain constants when truly domain-specific); avoid hard-coded strings.
- **Mapper + UpdateColumns pattern**: Prefer `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` + `sheet.Headers.UpdateColumns()` before adding formulas/formatting to guarantee correct column indexes.
- **Deterministic, unit-scoped tests**: Default tests to unit scope under `*.Tests/Unit`; use seeds and validation helpers to avoid flaky time/random/network dependencies.
- **Testing best practices**: Unit tests belong in the corresponding `*.Tests/Unit` project folders; keep tests deterministic, narrow in scope, and prefer validation helpers over brittle assertions.
- **Minimal optional attributes**: Use `ColumnOrder`/`SheetOrder` only when necessary; default ordering comes from declaration order.
- **Maintain reusability and clarity**: Favor small, composable methods, clear naming, and consistent JSON property usage across entities.

### Key Design Principles

1. **Optional Complexity**: Use explicit ordering only when needed
2. **Sensible Defaults**: Property declaration order is the default behavior
3. **Single Source of Truth**: Centralized configuration and ordering
4. **Type Safety**: Constant references for headers and sheet names
5. **Validation**: Build-time checks for configuration errors

---

## Core Concepts

### Header Management

**Central Header Repository**: All available headers are defined as constants for consistency:

```csharp
// Complete header inventory - use these constants exclusively
SheetsConfig.HeaderNames.Address         // Standard address field
SheetsConfig.HeaderNames.AddressStart    // Start address for trips
SheetsConfig.HeaderNames.AddressEnd      // End address for trips
SheetsConfig.HeaderNames.Pay            // Payment amount
SheetsConfig.HeaderNames.TotalGrand     // Grand total calculation
SheetsConfig.HeaderNames.AmountPerTime  // Calculated hourly rate
SheetsConfig.HeaderNames.VisitFirst     // First visit date
SheetsConfig.HeaderNames.DaysPerVisit   // Visit frequency metric
// ... complete catalog available in SheetsConfig.HeaderNames
```

### Column Ordering Strategy

**Default Behavior**: Properties use declaration order
**Optional Override**: Add `ColumnOrder` attribute only when specific positioning is needed

```csharp
public class DataEntity
{
    // Default: uses property declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";
    
    // Optional: explicit positioning when needed
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
}
```

### Sheet Tab Ordering Strategy

**Explicit Array Ordering**: Sheet order determined by explicit array for library safety

```csharp
/// <summary>
/// Sheet names with explicit ordering in _allSheetNames array
/// </summary>
public static class SheetNames
{
    // Sheet name constants (order not significant)
    public const string Trips = "Trips";
    public const string Shifts = "Shifts";
    public const string Expenses = "Expenses";
    // ... other constants
}

/// <summary>
/// Explicit ordering - this is the definitive source of truth
/// </summary>
private static readonly List<string> _allSheetNames = new()
{
    // Primary data entry sheets (leftmost tabs)
    SheetNames.Trips,
    SheetNames.Shifts,
    SheetNames.Expenses,
    
    // Reference data sheets (middle tabs)
    SheetNames.Addresses,
    SheetNames.Names,
    // ... other sheets in desired order
    
    // Administrative sheets (rightmost tabs)
    SheetNames.Setup
};
```

**Why Explicit Array Ordering?**
- **Library Safe**: No reflection dependencies that can break in different contexts
- **Deterministic**: Same order every time, regardless of compilation environment
- **AOT Compatible**: Works with ahead-of-time compilation and IL trimming
- **Explicit Intent**: Clear, readable ordering that's easy to maintain
- **Validation**: Simple validation ensures array stays synchronized with constants
- **Performance**: No reflection overhead at runtime

**Essential Test Coverage**:
Only basic tests are needed to cover the core functionality:
- Order validation (first, last, count)
- Case-insensitive name matching
- Invalid name handling
- Synchronization between constants and explicit array

The explicit array approach is simple and reliable enough that extensive testing is unnecessary.

---

## Architecture

### Component Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        SheetsConfig                              │
│  - Defines all sheet structures                                 │
│  - References HeaderNames constants                             │
│  - Entity-driven header generation                              │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Entity Classes                              │
│  - Domain objects representing data rows                        │
│  - Optional ColumnOrder attributes                              │
│  - Property declaration order as default                        │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Mapper Classes                              │
│  - Translate entities ↔ Google Sheets data                     │
│  - Configure formulas and formatting                            │
│  - Entity-driven column ordering                                │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Google Sheets API                             │
│  - Reads/writes spreadsheet data                                │
│  - Applies formatting and formulas                              │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Sheet Configuration (`SheetsConfig`)

**Purpose**: Centralized definition of sheet structures and properties

**Entity-Driven Implementation**:
```csharp
public static SheetModel ExampleSheet => new()
{
    Name = SheetNames.Example,
    CellColor = ColorEnum.LIGHT_CYAN,
    TabColor = ColorEnum.CYAN,
    FreezeColumnCount = 1,
    FreezeRowCount = 1,
    ProtectSheet = true,
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ExampleEntity>()
};
```

**Sheet Tab Ordering**: Entity-driven sheet order for consistent tab layout:
```csharp
var allSheets = SheetsConfig.SheetUtilities.GetAllSheetNames();
// Returns: [Trips, Shifts, Expenses, Addresses, Names, Places, ...]
// Order determined by constants declaration in SheetNames
```

**Responsibilities**:
- Define sheet visual properties (colors, frozen rows/columns)
- Automatically generate headers from entity attributes
- Use constants-based sheet ordering
- Set protection and validation rules

### 2. Entity Classes

**Purpose**: Domain objects representing data rows with optional column ordering

**Flattened Entity Design**:
```csharp
public class DataEntity : BaseEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    // Properties without ColumnOrder use declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";
    
    // Use ColumnOrder only for specific positioning
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
}
```

**Characteristics**:
- Use `ColumnOrder` **only when specific positioning is needed**
- Properties without `ColumnOrder` use declaration order
- Reference `SheetsConfig.HeaderNames` constants
- Include `RowId` for Google Sheets row mapping
- Use JSON property names for serialization
- Flattened design for precise column control

### 3. Sheet Container Entity

**Purpose**: Container for all sheet data with constants-based ordering

**Clean SheetEntity (No Ordering Attributes)**:
```csharp
public class SheetEntity
{
    // No ordering attributes needed - order from constants
    [JsonPropertyName("trips")]
    public List<TripEntity> Trips { get; set; } = [];

    [JsonPropertyName("shifts")]
    public List<ShiftEntity> Shifts { get; set; } = [];
    
    [JsonPropertyName("expenses")]
    public List<ExpenseEntity> Expenses { get; set; } = [];
    
    // ... all other sheets
}
```

### 4. Mapper Classes

**Purpose**: Translate between entities and Google Sheets data with automatic ordering and simplified configuration.

**Updated Configuration**:
```csharp
public static class ExampleMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ExampleSheet;  // Headers auto-generated
        sheet.Headers.UpdateColumns();         // Set column indexes

        ConfigureFormulas(sheet);

        return sheet;
    }

    // Google Sheets → Entity
    public static List<ExampleEntity> MapFromRangeData(IList<IList<object>> values)
    {
        return MapperHelper.MapFromRangeData<ExampleEntity>(values);
    }

    // Entity → Google Sheets
    public static IList<IList<object?>> MapToRangeData(
        List<ExampleEntity> entities, 
        IList<object> headers)
    {
        return MapperHelper.MapToRangeData(entities, headers);
    }

    private static void ConfigureFormulas(SheetModel sheet)
    {
        // Add formulas, formatting, validation
    }
}
```

### 5. Header Management Helpers

**Purpose**: Parse and extract data from Google Sheets rows

**Key Functions**:
```csharp
// Parse header row into column index dictionary
Dictionary<int, string> ParserHeader(IList<object> sheetHeader)

// Extract typed values using column names
string GetStringValue(string columnName, IList<object> values, Dictionary<int, string> headers)
int GetIntValue(string columnName, IList<object> values, Dictionary<int, string> headers)
decimal GetDecimalValue(string columnName, IList<object> values, Dictionary<int, string> headers)
```

### 6. Entity-Driven Configuration Helpers

**Purpose**: Extract and validate optional ordering from entity attributes

**Key Classes**:
```csharp
// Generate sheet headers from entity (respects optional ordering)
EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()

// Extract and apply column order (respects optional ordering)
EntityColumnOrderHelper.GetColumnOrderFromEntity<T>(sheetHeaders)

// Validate entity ColumnOrder mappings
EntityColumnOrderHelper.ValidateEntityHeaderMapping<T>(availableHeaders)

// Extract sheet order from constants
EntitySheetOrderHelper.GetSheetOrderFromEntity<T>()

// Validate entity SheetOrder mappings
EntitySheetOrderHelper.ValidateEntitySheetMapping<T>(availableSheets)
```

---

## Implementation Guide

### Workflow: Creating a New Sheet

**Step 1: Define Entity with Optional Ordering**

Start with property declaration order, add `ColumnOrder` only when needed:

```csharp
public class NewSheetEntity : BaseEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    // Default: declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    // Optional: specific positioning
    [JsonPropertyName("amount")]
    [ColumnOrder(SheetsConfig.HeaderNames.Amount)]
    public decimal Amount { get; set; }
}
```

**Step 2: Add Sheet to Constants (Optional Positioning)**

Add to `SheetsConfig.SheetNames` in desired tab order:

```csharp
public static class SheetNames
{
    // Existing sheets...
    public const string Shifts = "Shifts";
    
    // Add your sheet here - position determines tab order
    public const string NewSheet = "NewSheet";
    
    // Existing sheets...
    public const string Expenses = "Expenses";
}
```

**Step 3: Add to SheetEntity Container**

```csharp
public class SheetEntity
{
    // Existing sheets...
    
    [JsonPropertyName("newSheet")]
    public List<NewSheetEntity> NewSheet { get; set; } = [];
}
```

**Step 4: Define Configuration in SheetsConfig**

```csharp
public static SheetModel NewSheet => new()
{
    Name = SheetNames.NewSheet,
    TabColor = ColorEnum.BLUE,
    CellColor = ColorEnum.LIGHT_GRAY,
    FreezeColumnCount = 1,
    FreezeRowCount = 1,
    ProtectSheet = true,
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<NewSheetEntity>()
};
```

**Step 5: Implement Mapper**

```csharp
public static class NewSheetMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.NewSheet;
        sheet.Headers.UpdateColumns();  // Required for formulas
        
        // Configure formulas as needed
        ConfigureFormulas(sheet);
        
        return sheet;
    }
    
    public static List<NewSheetEntity> MapFromRangeData(IList<IList<object>> values)
    {
        // Map Google Sheets data to entities
    }
    
    public static IList<IList<object?>> MapToRangeData(
        List<NewSheetEntity> entities, 
        IList<object> headers)
    {
        // Map entities to Google Sheets data
    }
    
    private static void ConfigureFormulas(SheetModel sheet)
    {
        // Add formulas, formatting, validation
    }
}
```

### Column Order Management

**Entity-Driven Approach**:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.ExampleSheet;  // Headers from entity
    sheet.Headers.UpdateColumns();         // Sets indexes: A, B, C...
    
    // Now safe to reference columns for formulas
    var keyRange = sheet.GetLocalRange(HeaderEnum.KEY_FIELD.GetDescription());
}
```

### Sheet Tab Order Management

**Explicit Array Ordering**:
```csharp
// GetAllSheetNames() uses constants declaration order
var sheets = SheetsConfig.SheetUtilities.GetAllSheetNames();
// Returns sheets in SheetNames declaration order
```

---

## Implementation Patterns

### Adding Optional ColumnOrder to Entities

```csharp
public class DataEntity
{
    // Default: declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";
    
    // Explicit positioning when needed
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
    
    // Resulting order: Date, Service, Pay (as positioned)
}
```

### Adding Optional SheetOrder to SheetEntity

```csharp
public class SheetEntity
{
    // Default behavior - property declaration order
    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.SheetNames.Trips)]
    public List<TripEntity> Trips { get; set; } = [];
    
    [JsonPropertyName("shifts")]
    [SheetOrder(SheetsConfig.SheetNames.Shifts)]
    public List<ShiftEntity> Shifts { get; set; } = [];
    
    // Use explicit order only when needed
    [JsonPropertyName("setup")]
    [SheetOrder(99, SheetsConfig.SheetNames.Setup)]  // Force to end
    public List<SetupEntity> Setup { get; set; } = [];
}
```

### Using Entity Ordering in SheetsConfig

```csharp
public static SheetModel DataSheet => new()
{
    Name = SheetNames.Data,
    TabColor = ColorEnum.BLUE,
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DataEntity>()
    // Respects property order + optional ColumnOrder attributes
};
```

### Using Entity Ordering in Mappers

```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.DataSheet;     // Headers from entity
    sheet.Headers.UpdateColumns();         // Required for formulas
    
    ConfigureSheetFormulas(sheet);
    
    return sheet;
}
```

### Validation in Tests

```csharp
[Fact]
public void ValidateEntityConfiguration()
{
    // Validate column order configuration
    var columnErrors = EntitySheetConfigHelper
        .ValidateEntityForSheetGeneration<DataEntity>();
    Assert.Empty(columnErrors);
    
    // Validate sheet order configuration
    var sheetErrors = EntitySheetOrderHelper
        .ValidateEntitySheetMapping<SheetEntity>(availableSheets);
    Assert.Empty(sheetErrors);
}
```

---

## Migration Guide

### For New Sheets

1. Define entity properties in desired declaration order
2. **Add `ColumnOrder` only when specific positioning is needed**
3. **Add sheet to `SheetNames` constants in desired tab position**
4. Use `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` in SheetsConfig
5. Simplify mapper by removing manual ordering code

### For Existing Sheets

1. Review entity properties and reorder declarations as desired
2. Add `ColumnOrder` attributes only where specific positioning is needed
3. Review `SheetNames` constants and reorder declarations for desired tabs
4. Update SheetsConfig to use entity-driven generation
5. Remove manual header definitions from mappers
6. Test to ensure column and sheet order match expectations

---

## Best Practices

### Column Ordering

1. **Start with declaration order**: Let property sequence determine default order
2. **Use attributes sparingly**: Add `ColumnOrder` only when you need specific positioning
3. **Always use constants**: Reference `SheetsConfig.HeaderNames` in attributes
4. **Document explicit ordering**: When using explicit numbers, document why

### Sheet Tab Ordering

1. **Use constants declaration order**: Position in `SheetNames` determines tab order
2. **Group related sheets**: Keep related sheets together in constants
3. **Primary sheets first**: Data entry sheets at the top
4. **Admin sheets last**: Setup/config sheets at the bottom

### Configuration

1. **Validate at build time**: Use helper validation methods in tests
2. **Keep UpdateColumns()**: Still needed for formula column references
3. **Entity-driven headers**: Always use `GenerateHeadersFromEntity<T>()`
4. **Centralized configuration**: All sheet properties in SheetsConfig

### Code Quality

1. **Type safety**: Use constants for all header and sheet references
2. **Single source of truth**: Entity defines order (when specified)
3. **Prefer defaults**: Most scenarios work with declaration order
4. **Document deviations**: Explain when you override default ordering

---

## Benefits Summary

The **optional entity-driven ordering system** provides:

- ✅ **Sensible defaults** using property declaration order
- ✅ **Optional complexity** through explicit ordering when needed
- ✅ **Single source of truth** for ordering (in entities/constants)
- ✅ **Strong typing** through constant references
- ✅ **Precise control** via optional attributes
- ✅ **Constants-based sheet ordering** for clean, simple tab management
- ✅ **Simplified configuration** in SheetsConfig and mappers
- ✅ **Validation** to ensure valid header/sheet references
- ✅ **Backward compatibility** with existing systems
- ✅ **Clean code** - most entities don't need ordering attributes

### Comparison: Old vs New

**OLD: Manual Configuration**
```csharp
// Manual header list
Headers = [
    new SheetCellModel { Name = HeaderNames.Field1 },
    new SheetCellModel { Name = HeaderNames.Field2 },
    // ... manual list, easy to get out of sync
]

// Hardcoded sheet order
public static List<string> GetAllSheetNames() => [
    SheetNames.Trips, SheetNames.Shifts, ...
];
```

**NEW: Entity-Driven with Optional Ordering**
```csharp
// Entity-driven generation
Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<EntityType>()
// Respects declaration order + optional ColumnOrder attributes

// Constants-driven sheet order
public static List<string> GetAllSheetNames() =>
    ConstantsOrderHelper.GetOrderFromConstants(typeof(SheetNames));
// Uses declaration order from SheetNames constants
```

---

## Key Features

### Automatic Header Generation
- Headers generated from entity properties
- Respects declaration order by default
- Optional `ColumnOrder` for specific positioning
- Type-safe constant references

### Constants-Based Sheet Ordering
- Sheet order from `SheetNames` constants declaration
- Single place to define names and order
- No attributes needed on `SheetEntity`
- Easy to understand and maintain

### Validation and Error Prevention
- Build-time validation of entity configuration
- Catches invalid header/sheet references
- Ensures ordering consistency

### Simplified Maintenance
- Less boilerplate code
- Centralized configuration
- Easy to reorder columns/sheets
- Clear intent through optional attributes

This approach provides sensible defaults while maintaining the explicit control that complex Google Sheets formulas require, significantly improving maintainability and reducing configuration errors.

# Google Sheets Management System Documentation

## Overview

The RaptorSheets system manages Google Sheets through a layered architecture that separates concerns between configuration, data modeling, and business logic. The system handles complex spreadsheets with automated formulas, cross-sheet references, and **optional** column and sheet ordering.

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

**Constants-Based Ordering**: Sheet order determined by constant declaration sequence

```csharp
/// <summary>
/// Sheet names with implicit ordering based on declaration order
/// </summary>
public static class SheetNames
{
    // Primary data entry sheets (declared first = leftmost tabs)
    public const string Trips = "Trips";
    public const string Shifts = "Shifts";
    public const string Expenses = "Expenses";
    
    // Reference data sheets (middle tabs)
    public const string Addresses = "Addresses";
    public const string Names = "Names";
    public const string Places = "Places";
    
    // Analysis/summary sheets (right-side tabs)
    public const string Daily = "Daily";
    public const string Weekly = "Weekly";
    
    // Administrative sheets (rightmost tabs)
    public const string Setup = "Setup";
}
```

**Why Constants-Based Ordering?**
- **Dual Purpose**: Constants define both names and order
- **Single Location**: Order visible in one place
- **No Magic Numbers**: Order is implicit from declaration
- **Easy Maintenance**: Reordering = moving declarations
- **Clean Code**: No attributes needed on `SheetEntity`

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

**Purpose**: Translate between entities and Google Sheets data with automatic ordering

**Simplified Configuration**:
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
    
    // Entity → Google Sheets
    public static IList<IList<object?>> MapToRangeData(
        List<ExampleEntity> entities, 
        IList<object> headers)
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

**Constants-Based Ordering**:
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

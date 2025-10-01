# Google Sheets Management System Documentation

## Current Architecture Overview

The RaptorSheets system manages Google Sheets through a layered architecture that separates concerns between configuration, data modeling, and business logic. The system is designed to handle complex spreadsheets with automated formulas, cross-sheet references, and **optional** column and sheet ordering.

## Core Components

### 1. Sheet Configuration (`SheetsConfig`)
**Purpose**: Centralized definition of sheet structures and properties

**NEW: Entity-Driven Implementation**:
```csharp
public static SheetModel ExampleSheet => new()
{
    Name = SheetNames.Example,
    CellColor = ColorEnum.LIGHT_CYAN,
    TabColor = ColorEnum.CYAN,
    FreezeColumnCount = 1,
    FreezeRowCount = 1,
    ProtectSheet = true,
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ExampleEntity>()  // Auto-generated from entity
};
```

**Central Header Repository**: SheetsConfig provides a comprehensive catalog of all available headers for consistency across sheets:
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

**Sheet Tab Ordering**: SheetsConfig maintains entity-driven sheet order for consistent tab layout:
```csharp
// Entity-driven tab order (left to right in spreadsheet)
var allSheets = SheetsConfig.SheetUtilities.GetAllSheetNames();
// Returns entity-defined order from SheetEntity: [Trips, Shifts, Expenses, Addresses, Names, Places, Regions, Services, Types, Daily, Weekdays, Weekly, Monthly, Yearly, Setup]
```

**Responsibilities**:
- Define sheet visual properties (colors, frozen rows/columns)
- Automatically generate headers from entity `ColumnOrder` attributes
- Use entity-driven sheet ordering from `SheetEntity`
- Set protection and validation rules

### 2. Entity Classes with Optional ColumnOrder Attributes
**Purpose**: Domain objects representing data rows in sheets with optional column ordering

**Optional Attribute-Based Column Ordering**:
```csharp
public class ExampleEntity : BaseEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    // Optional explicit ordering - use only when you need specific positioning
    [JsonPropertyName("keyField")]
    [ColumnOrder(SheetsConfig.HeaderNames.KeyField)]  // References header constants
    public string KeyField { get; set; } = "";
    
    // No ColumnOrder attribute - will use property declaration order
    [JsonPropertyName("measureField")]
    public decimal MeasureField { get; set; }
    
    // Mix of explicit and default ordering as needed
}
```

**Flattened Entity Design with Optional Ordering**: Modern entities define properties with ordering only when needed:
```csharp
public class DataEntity
{
    // Properties without ColumnOrder use declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";
    
    // Use ColumnOrder only when you need specific positioning
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
    
    // Resulting column order: Date, Service, Pay (declaration order with Pay positioned as specified)
}
```

**Characteristics**:
- Use `ColumnOrder` attributes **only when you need specific positioning**
- Properties without `ColumnOrder` use natural declaration order
- Reference `SheetsConfig.HeaderNames` constants when using attributes
- Include `RowId` for Google Sheets row mapping
- Use JSON property names for serialization
- Flattened design for precise column control when needed

### 3. Sheet Tab Ordering with Constants-Based Ordering
**Purpose**: Define sheet tab order in workbooks using constants declaration order

**Constants-Based Sheet Ordering**:
```csharp
/// <summary>
/// Sheet names with implicit ordering based on declaration order
/// </summary>
public static class SheetNames
{
    // Primary data entry sheets (declared first for highest priority)
    public const string Trips = "Trips";
    public const string Shifts = "Shifts";
    public const string Expenses = "Expenses";
    
    // Reference data sheets (depend on primary data)
    public const string Addresses = "Addresses";
    public const string Names = "Names";
    public const string Places = "Places";
    // ... more reference sheets
    
    // Analysis/summary sheets (depend on primary and reference data)
    public const string Daily = "Daily";
    public const string Weekly = "Weekly";
    // ... more analysis sheets
    
    // Administrative sheets (lowest priority, declared last)
    public const string Setup = "Setup";
}
```

**Clean SheetEntity (No Ordering Attributes)**:
```csharp
public class SheetEntity
{
    // No ordering attributes needed - order determined by constants
    [JsonPropertyName("trips")]
    public List<TripEntity> Trips { get; set; } = [];

    [JsonPropertyName("shifts")]
    public List<ShiftEntity> Shifts { get; set; } = [];
    
    // ... all other sheets
}
```

### 4. Mapper Classes with Entity-Driven Ordering
**Purpose**: Translate between entities and Google Sheets data structures with automatic column ordering

**Simplified Configuration**:
```csharp
public static class ExampleMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ExampleSheet;  // Headers auto-generated from entity
        sheet.Headers.UpdateColumns();         // Set column indexes for formula references

        // Configure formulas and formatting as needed
        ConfigureFormulas(sheet);
        
        return sheet;
    }
}
```

**Core Methods**:
```csharp
public static class ExampleMapper
{
    // Google Sheets data → Entity objects
    public static List<ExampleEntity> MapFromRangeData(IList<IList<object>> values)
    
    // Entity objects → Google Sheets data
    public static IList<IList<object?>> MapToRangeData(List<ExampleEntity> entities, IList<object> headers)
    
    // Configure sheet formulas and formatting with entity-driven ordering
    public static SheetModel GetSheet()
}
```

### 5. Header Management (`HeaderHelpers`)
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
**Purpose**: Extract and validate optional column/sheet ordering from entity attributes

**Key Classes**:
```csharp
// Generate sheet headers from entity ColumnOrder attributes (optional)
EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()

// Extract column order and apply to existing headers (respects optional ordering)
EntityColumnOrderHelper.GetColumnOrderFromEntity<T>(sheetHeaders)

// Validate entity ColumnOrder attribute mappings
EntityColumnOrderHelper.ValidateEntityHeaderMapping<T>(availableHeaders)

// Extract sheet order from SheetOrder attributes (respects optional ordering)
EntitySheetOrderHelper.GetSheetOrderFromEntity<T>()

// Validate entity SheetOrder attribute mappings
EntitySheetOrderHelper.ValidateEntitySheetMapping<T>(availableSheets)
```

## Current Workflow

### Sheet Creation Process

1. **Define Entity with Optional Column Ordering**: Create entity class, use `ColumnOrder` attributes only when needed
2. **Define Optional Sheet Tab Ordering**: Update `SheetEntity` with `SheetOrder` attributes only when specific positioning is needed
3. **Define Configuration**: Add static `SheetModel` to `SheetsConfig` using entity-driven generation
4. **Implement Mapper**: Create mapper with simplified configuration
5. **Configure Formulas**: Set up formulas and formatting in `GetSheet()` method

### Column Order Management

**Entity-Driven Approach with Optional Ordering**:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.ExampleSheet;  // Headers from entity (declaration order + optional explicit order)
    sheet.Headers.UpdateColumns();         // Sets column indexes A, B, C, etc.
    
    // Now safe to reference columns by index for formulas
    var keyRange = sheet.GetLocalRange(HeaderEnum.KEY_FIELD.GetDescription());
}
```

### Sheet Tab Order Management

**Constants-Based Sheet Ordering**:
```csharp
// SheetsConfig.SheetUtilities.GetAllSheetNames() uses:
ConstantsOrderHelper.GetOrderFromConstants(typeof(SheetsConfig.SheetNames))
// Returns sheets in the order they are declared in SheetsConfig.SheetNames
```

**Benefits**:
- **Simple Dual-Purpose**: Constants define both names and order
- **Single Source of Truth**: Order defined in one place (constants declaration)
- **No Magic Numbers**: Order is implicit from declaration sequence
- **Easy to Understand**: Order is visible in constants file
- **Easy to Maintain**: Reordering is just moving constant declarations
- **Clean Code**: No attributes needed on entities

## Implementation Patterns

### Adding Optional ColumnOrder Attributes to Entities

```csharp
public class DataEntity
{
    // Default behavior - uses property declaration order
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";
    
    // Use ColumnOrder only when you need specific positioning
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
    
    // Mix of default and explicit ordering
}
```

### Adding Optional SheetOrder Attributes to SheetEntity

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
    CellColor = ColorEnum.LIGHT_GRAY,
    FreezeColumnCount = 1,
    FreezeRowCount = 1,
    ProtectSheet = true,
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DataEntity>()  // Respects optional ordering
};
```

### Using Entity Ordering in Mappers

```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.DataSheet;     // Headers from entity (with optional ordering)
    sheet.Headers.UpdateColumns();         // Required for formula references
    
    // Configure formulas as usual...
    ConfigureSheetFormulas(sheet);
    
    return sheet;
}
```

## Benefits Achieved

1. **Optional Complexity**: Use explicit ordering only when needed
2. **Sensible Defaults**: Property declaration order is the default behavior
3. **Constant References**: Uses `SheetsConfig.HeaderNames` and `SheetsConfig.SheetNames` when needed  
4. **Precise Control**: Explicit positioning available when required
5. **Validation**: Build-time checks for invalid header/sheet references
6. **No Magic Numbers**: Only specify numbers when positioning is important
7. **Centralized Sheet Ordering**: Single place to define tab order when needed
8. **Clean Code**: Most entities don't need explicit ordering attributes

## Key Features

### Automatic Header Generation with Optional Ordering
```csharp
// OLD: Manual header definition
Headers = [
    new SheetCellModel { Name = HeaderNames.Field1 },
    new SheetCellModel { Name = HeaderNames.Field2 },
    // ... manual list
]

// NEW: Entity-driven generation with optional ordering
Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<EntityType>()
// Respects property declaration order + optional ColumnOrder attributes
```

### Entity-Driven Sheet Ordering with Optional Positioning
```csharp
// OLD: Hardcoded array
public static List<string> GetAllSheetNames() => [
    SheetNames.Trips, SheetNames.Shifts, ...
];

// NEW: Entity-driven ordering with optional SheetOrder attributes
public static List<string> GetAllSheetNames() =>
    EntitySheetOrderHelper.GetSheetOrderFromEntity<SheetEntity>();
// Respects property declaration order + optional SheetOrder attributes
```

### Validation and Error Prevention
```csharp
// Validates entity configuration at build/test time
var columnErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<EntityType>();
var sheetErrors = EntitySheetOrderHelper.ValidateEntitySheetMapping<SheetEntity>(availableSheets);
```

## Migration Guide

### For New Sheets
1. Define entity properties in desired order
2. **Add `ColumnOrder` attributes only when you need specific positioning**
3. **Add `SheetOrder` attribute to `SheetEntity` only if you need specific tab positioning**
4. Use `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` in SheetsConfig
5. Simplify mapper by removing manual ordering code

### For Existing Sheets
1. Review entity properties and ensure they're in desired declaration order
2. Add `ColumnOrder` attributes only where specific positioning is needed
3. Update SheetsConfig to use entity-driven generation
4. Remove unnecessary manual header definitions from mappers
5. Test to ensure column order matches expectations

## Best Practices

1. **Start with property order**: Let property declaration determine default order
2. **Use attributes sparingly**: Add `ColumnOrder`/`SheetOrder` only when you need specific positioning
3. **Always use constants**: Reference `SheetsConfig.HeaderNames` when using `ColumnOrder` attributes
4. **Use constants for sheets**: Reference `SheetsConfig.SheetNames` when using `SheetOrder` attributes  
5. **Validate at build time**: Use helper validation methods in tests
6. **Keep UpdateColumns()**: Still needed for formula column references
7. **Document explicit ordering**: When you use explicit order numbers, document why
8. **Prefer declaration order**: Most scenarios work fine with natural property order

## Summary

The **optional entity-driven ordering system** provides:

- **Sensible defaults** using property declaration order
- **Optional complexity** through explicit ordering attributes when needed
- **Single source of truth** for ordering (in entities, when specified)
- **Strong typing** through header and sheet constant references
- **Precise column control** via optional ColumnOrder attributes
- **Optional sheet positioning** via optional SheetOrder attributes
- **Simplified configuration** in SheetsConfig and mappers
- **Validation** to ensure entity attributes reference valid headers/sheets
- **Backward compatibility** with existing formula and configuration systems
- **Clean code** - most entities don't need any ordering attributes

This approach provides sensible defaults while maintaining the explicit control that complex Google Sheets formulas require. The system significantly improves maintainability by removing the need for explicit ordering in most cases, while still allowing precise control when needed.

# Google Sheets Management System Documentation

## Current Architecture Overview

The RaptorSheets system manages Google Sheets through a layered architecture that separates concerns between configuration, data modeling, and business logic. The system is designed to handle complex spreadsheets with automated formulas, cross-sheet references, and strict column ordering.

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
// Returns entity-defined order from SheetOrderEntity: [Trips, Shifts, Expenses, Addresses, Names, Places, Regions, Services, Types, Daily, Weekdays, Weekly, Monthly, Yearly, Setup]
```

**Responsibilities**:
- Define sheet visual properties (colors, frozen rows/columns)
- Automatically generate headers from entity `ColumnOrder` attributes
- Use entity-driven sheet ordering from `SheetOrderEntity`
- Set protection and validation rules

### 2. Entity Classes with ColumnOrder Attributes
**Purpose**: Domain objects representing data rows in sheets with built-in column ordering

**Attribute-Based Column Ordering**:
```csharp
public class ExampleEntity : BaseEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    [JsonPropertyName("keyField")]
    [ColumnOrder(SheetsConfig.HeaderNames.KeyField)]  // References header constants
    public string KeyField { get; set; } = "";
    
    [JsonPropertyName("measureField")]
    [ColumnOrder(SheetsConfig.HeaderNames.MeasureField)]
    public decimal MeasureField { get; set; }
    
    // For flattened entities: all properties defined directly in correct order
}
```

**Flattened Entity Design**: Modern entities define all properties directly to control exact column ordering:
```csharp
public class DataEntity
{
    [JsonPropertyName("date")]
    [ColumnOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("trips")]
    [ColumnOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }
    
    // Financial properties in exact position needed
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }
    
    [JsonPropertyName("tip")]
    [ColumnOrder(SheetsConfig.HeaderNames.Tips)]
    public decimal? Tip { get; set; }
    
    // Resulting column order: Date, Trips, Pay, Tip...
}
```

**Characteristics**:
- Use `ColumnOrder` attributes to define column order
- Reference `SheetsConfig.HeaderNames` constants exclusively
- Include `RowId` for Google Sheets row mapping
- Use JSON property names for serialization
- Flattened design for precise column control

### 3. Sheet Tab Ordering with SheetOrderEntity
**Purpose**: Central definition of sheet tab order in workbooks

**SheetOrder Attributes for Tabs**:
```csharp
public class SheetOrderEntity
{
    [JsonPropertyName("trips")]
    [SheetOrder(0, SheetsConfig.SheetNames.Trips)]
    public bool Trips { get; set; } = true;

    [JsonPropertyName("shifts")]
    [SheetOrder(1, SheetsConfig.SheetNames.Shifts)]
    public bool Shifts { get; set; } = true;
    
    // ... more sheets in order
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
**Purpose**: Extract and validate column/sheet ordering from entity attributes

**Key Classes**:
```csharp
// Generate sheet headers from entity ColumnOrder attributes
EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()

// Extract column order and apply to existing headers
EntityColumnOrderHelper.GetColumnOrderFromEntity<T>(sheetHeaders)

// Validate entity ColumnOrder attribute mappings
EntityColumnOrderHelper.ValidateEntityHeaderMapping<T>(availableHeaders)

// Extract sheet order from SheetOrder attributes
EntitySheetOrderHelper.GetSheetOrderFromEntity<T>()

// Validate entity SheetOrder attribute mappings
EntitySheetOrderHelper.ValidateEntitySheetMapping<T>(availableSheets)
```

## Current Workflow

### Sheet Creation Process

1. **Define Entity with Column Ordering**: Create entity class with `ColumnOrder` attributes
2. **Define Sheet Tab Ordering**: Update `SheetOrderEntity` with `SheetOrder` attributes
3. **Define Configuration**: Add static `SheetModel` to `SheetsConfig` using entity-driven generation
4. **Implement Mapper**: Create mapper with simplified configuration
5. **Configure Formulas**: Set up formulas and formatting in `GetSheet()` method

### Column Order Management

**Entity-Driven Approach**:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.ExampleSheet;  // Headers from entity ColumnOrder attributes
    sheet.Headers.UpdateColumns();         // Sets column indexes A, B, C, etc.
    
    // Now safe to reference columns by index for formulas
    var keyRange = sheet.GetLocalRange(HeaderEnum.KEY_FIELD.GetDescription());
}
```

### Sheet Tab Order Management

**Entity-Driven Sheet Ordering**:
```csharp
// SheetsConfig.SheetUtilities.GetAllSheetNames() now uses:
EntitySheetOrderHelper.GetSheetOrderFromEntity<SheetOrderEntity>()
// Returns sheets in order defined by SheetOrder attributes: [Trips, Shifts, Expenses, ...]
```

**Benefits**:
- **Single Source of Truth**: Entity properties define both column and sheet order
- **References Constants**: Uses `SheetsConfig.HeaderNames` and `SheetsConfig.SheetNames` for consistency
- **Flattened Design**: Precise control over column positioning
- **No Hardcoding**: Eliminates order numbers or column letters
- **Readable**: Order is visible in entity definitions

### Data Flow

**Reading Data**:
```
Google Sheets Range Data → HeaderHelpers.ParserHeader() → Entity Properties
```

**Writing Data**:
```
Entity Properties → MapToRangeData/MapToRowData → Google Sheets Batch Update
```

**Sheet Generation**:
```
Entity ColumnOrder Attributes → EntitySheetConfigHelper → SheetsConfig → Mapper.GetSheet() → Formula Configuration → Google Sheets Creation
```

**Sheet Ordering**:
```
SheetOrderEntity SheetOrder Attributes → EntitySheetOrderHelper → SheetsConfig.SheetUtilities → Workbook Tab Order
```

## Implementation Patterns

### Adding ColumnOrder Attributes to Entities

```csharp
public class DataEntity
{
    [JsonPropertyName("date")]
    [ColumnOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    [ColumnOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";
    
    // All properties defined directly for precise control
}
```

### Adding SheetOrder Attributes to SheetOrderEntity

```csharp
public class SheetOrderEntity
{
    [JsonPropertyName("newSheet")]
    [SheetOrder(15, SheetsConfig.SheetNames.NewSheet)]  // Add to end
    public bool NewSheet { get; set; } = true;
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
    Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DataEntity>()
};
```

### Using Entity Ordering in Mappers

```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.DataSheet;     // Headers auto-generated from entity
    sheet.Headers.UpdateColumns();         // Required for formula references
    
    // Configure formulas as usual...
    ConfigureSheetFormulas(sheet);
    
    return sheet;
}
```

## Benefits Achieved

1. **Maintainable Order**: Column and sheet order visible in entity definitions
2. **Constant References**: Uses `SheetsConfig.HeaderNames` and `SheetsConfig.SheetNames` exclusively  
3. **Precise Control**: Flattened entities allow exact column positioning
4. **Validation**: Build-time checks for invalid header/sheet references
5. **No Magic Numbers**: Eliminates hardcoded positions
6. **Centralized Sheet Ordering**: Single place to define tab order

## Key Features

### Automatic Header Generation
```csharp
// OLD: Manual header definition
Headers = [
    new SheetCellModel { Name = HeaderNames.Field1 },
    new SheetCellModel { Name = HeaderNames.Field2 },
    // ... manual list
]

// NEW: Entity-driven generation
Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<EntityType>()
```

### Entity-Driven Sheet Ordering
```csharp
// OLD: Hardcoded array
public static List<string> GetAllSheetNames() => [
    SheetNames.Trips, SheetNames.Shifts, ...
];

// NEW: Entity-driven ordering
public static List<string> GetAllSheetNames() =>
    EntitySheetOrderHelper.GetSheetOrderFromEntity<SheetOrderEntity>();
```

### Validation and Error Prevention
```csharp
// Validates entity configuration at build/test time
var columnErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<EntityType>();
var sheetErrors = EntitySheetOrderHelper.ValidateEntitySheetMapping<SheetOrderEntity>(availableSheets);
```

## Migration Guide

### For New Sheets
1. Add `ColumnOrder` attributes to entity properties
2. **Add `SheetOrder` attribute to `SheetOrderEntity` if creating new sheet**
3. Use `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` in SheetsConfig
4. Simplify mapper by removing manual ordering code

### For Existing Sheets
1. Add `ColumnOrder` attributes to existing entity properties
2. Update SheetsConfig to use entity-driven generation
3. Remove manual header definitions from mappers
4. Test to ensure column order matches expectations

## Best Practices

1. **Always use constants**: Reference `SheetsConfig.HeaderNames` in `ColumnOrder` attributes
2. **Use constants for sheets**: Reference `SheetsConfig.SheetNames` in `SheetOrder` attributes  
3. **Validate at build time**: Use helper validation methods in tests
4. **Keep UpdateColumns()**: Still needed for formula column references
5. **Document changes**: Update entity documentation when changing order
6. **Flatten when needed**: Use flattened entities for precise column control

## Summary

The **dual entity-driven ordering system** provides:

- **Single source of truth** for both column and sheet order (in entities)
- **Strong typing** through header and sheet constant references
- **Precise column control** via flattened entity design
- **Centralized sheet ordering** via `SheetOrderEntity`
- **Simplified configuration** in SheetsConfig and mappers
- **Validation** to ensure entity attributes reference valid headers/sheets
- **Backward compatibility** with existing formula and configuration systems

This approach eliminates manual order management while maintaining the explicit control that complex Google Sheets formulas require. The system significantly improves maintainability and reduces the chance of ordering mismatches.

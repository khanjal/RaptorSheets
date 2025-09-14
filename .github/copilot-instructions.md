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

**Sheet Tab Ordering**: SheetsConfig maintains the definitive sheet order for consistent tab layout:
```csharp
// Predefined tab order (left to right in spreadsheet)
var allSheets = SheetsConfig.SheetUtilities.GetAllSheetNames();
// Returns in order: [Trips, Shifts, Expenses, Addresses, Names, Places, Regions, Services, Types, Daily, Weekdays, Weekly, Monthly, Yearly, Setup]
```

**Responsibilities**:
- Define sheet visual properties (colors, frozen rows/columns)
- Automatically generate headers from entity `SheetOrder` attributes
- Group common header patterns (`CommonIncomeHeaders`, `CommonTravelHeaders`)
- Set protection and validation rules

### 2. Entity Classes with SheetOrder Attributes
**Purpose**: Domain objects representing data rows in sheets with built-in column ordering

**Attribute-Based Column Ordering**:
```csharp
public class ExampleEntity : BaseEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    [JsonPropertyName("keyField")]
    [SheetOrder(SheetsConfig.HeaderNames.KeyField)]  // References header constants
    public string KeyField { get; set; } = "";
    
    [JsonPropertyName("measureField")]
    [SheetOrder(SheetsConfig.HeaderNames.MeasureField)]
    public decimal MeasureField { get; set; }
    
    // Inherits ordered properties from base classes automatically
}
```

**Inheritance-Aware Ordering**: The system respects inheritance hierarchy:
```csharp
// Base class properties
[SheetOrder(SheetsConfig.HeaderNames.BaseField1)]
public decimal? BaseField1 { get; set; }

[SheetOrder(SheetsConfig.HeaderNames.BaseField2)]
public decimal? BaseField2 { get; set; }

// Middle class properties
[SheetOrder(SheetsConfig.HeaderNames.MiddleField)]
public int MiddleField { get; set; }

// Derived class properties
[SheetOrder(SheetsConfig.HeaderNames.DerivedField)]
public string DerivedField { get; set; } = "";

// Resulting column order: BaseField1, BaseField2, MiddleField, DerivedField
```

**Characteristics**:
- Inherit from base classes for shared properties
- Include `RowId` for Google Sheets row mapping
- Use JSON property names for serialization
- Use `SheetOrder` attributes to define column order without hardcoding numbers
- May contain more or fewer properties than sheet columns

### 3. Mapper Classes with Entity-Driven Ordering
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

### 4. Header Management (`HeaderHelpers`)
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

### 5. Entity-Driven Configuration Helpers
**Purpose**: Extract and validate column ordering from entity attributes

**Key Classes**:
```csharp
// Generate sheet headers from entity attributes
EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()

// Extract column order and apply to existing headers
EntityColumnOrderHelper.GetColumnOrderFromEntity<T>(sheetHeaders)

// Validate entity attribute mappings
EntityColumnOrderHelper.ValidateEntityHeaderMapping<T>(availableHeaders)
```

## Current Workflow

### Sheet Creation Process

1. **Define Entity with Ordering**: Create entity class with `SheetOrder` attributes
2. **Define Configuration**: Add static `SheetModel` to `SheetsConfig` using entity-driven generation
3. **Implement Mapper**: Create mapper with simplified configuration
4. **Configure Formulas**: Set up formulas and formatting in `GetSheet()` method

### Column Order Management

**Entity-Driven Approach**:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.ExampleSheet;  // Headers from entity attributes
    sheet.Headers.UpdateColumns();         // Sets column indexes A, B, C, etc.
    
    // Now safe to reference columns by index for formulas
    var keyRange = sheet.GetLocalRange(HeaderEnum.KEY_FIELD.GetDescription());
}
```

**Benefits**:
- **Single Source of Truth**: Entity properties define column order
- **References Constants**: Uses `SheetsConfig.HeaderNames` for consistency
- **Inheritance Support**: Handles complex entity hierarchies automatically
- **No Hardcoding**: Eliminates order numbers or column letters
- **Readable**: Column order is visible in entity definition

**Why This Matters**:
- Enables formula references without hardcoding column letters
- Maintains correct order when mapping to/from Google Sheets
- Supports complex formulas that reference other columns
- Allows dynamic column insertion without breaking downstream references
- Column order is defined in entities and automatically applied

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
Entity SheetOrder Attributes → EntitySheetConfigHelper → SheetsConfig → Mapper.GetSheet() → Formula Configuration → Google Sheets Creation
```

## Implementation Patterns

### Adding SheetOrder Attributes to Entities

```csharp
public class DataEntity : BaseEntity
{
    [JsonPropertyName("date")]
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    [SheetOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";
    
    // Inherits ordered properties from base classes automatically
    // Final order: BaseField1, BaseField2, Date, Service, ...
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

1. **Maintainable Order**: Column order visible in entity definition
2. **Constant References**: Uses `SheetsConfig.HeaderNames` exclusively  
3. **Inheritance Support**: Base class properties ordered correctly
4. **Validation**: Build-time checks for invalid header references
5. **No Magic Numbers**: Eliminates hardcoded column positions
6. **Flexible**: Can still add additional headers if needed

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

### Inheritance Support
```csharp
// Automatically handles complex inheritance chains
// Base → Middle → Derived class property ordering
var headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DerivedEntity>();
// Returns: [BaseProps..., MiddleProps..., DerivedProps...]
```

### Validation and Error Prevention
```csharp
// Validates entity configuration at build/test time
var errors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<EntityType>();
if (errors.Any()) throw new InvalidOperationException(string.Join(", ", errors));
```

## Migration Guide

### For New Sheets
1. Add `SheetOrder` attributes to entity properties
2. Use `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` in SheetsConfig
3. Simplify mapper by removing manual ordering code

### For Existing Sheets
1. Add `SheetOrder` attributes to existing entity properties
2. Update SheetsConfig to use entity-driven generation
3. Remove manual header definitions and validation code from mappers
4. Test to ensure column order matches expectations

## Best Practices

1. **Always use constants**: Reference `SheetsConfig.HeaderNames` in `SheetOrder` attributes
2. **Test inheritance**: Verify column order with complex entity hierarchies
3. **Validate at build time**: Use helper validation methods in tests
4. **Keep UpdateColumns()**: Still needed for formula column references
5. **Document changes**: Update entity documentation when changing column order

## Summary

The **entity-driven column ordering system** provides:

- **Single source of truth** for column order (in entities)
- **Strong typing** through header constant references
- **Automatic inheritance handling** for complex entity hierarchies
- **Simplified configuration** in SheetsConfig and mappers
- **Validation** to ensure entity attributes reference valid headers
- **Backward compatibility** with existing formula and configuration systems

This approach eliminates manual column management while maintaining the explicit control that complex Google Sheets formulas require. The system significantly improves maintainability and reduces the chance of column order mismatches.

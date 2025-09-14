# Google Sheets Management System Documentation

## Current Architecture Overview

The RaptorSheets system manages Google Sheets through a layered architecture that separates concerns between configuration, data modeling, and business logic. The system is designed to handle complex spreadsheets with automated formulas, cross-sheet references, and strict column ordering.

## Core Components

### 1. Sheet Configuration (`SheetsConfig`)
**Purpose**: Centralized definition of sheet structures and properties

**Current Implementation**:
```csharp
public static SheetModel AddressSheet => new()
{
    Name = SheetNames.Addresses,
    CellColor = ColorEnum.LIGHT_CYAN,
    TabColor = ColorEnum.CYAN,
    FreezeColumnCount = 1,
    FreezeRowCount = 1,
    ProtectSheet = true,
    Headers = [
        new SheetCellModel { Name = HeaderNames.Address },
        .. CommonTripSheetHeaders  // Shared header patterns
    ]
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
- Specify column headers and their order
- Group common header patterns (`CommonIncomeHeaders`, `CommonTravelHeaders`)
- Set protection and validation rules

### 2. Entity Classes with SheetOrder Attributes
**Purpose**: Domain objects representing data rows in sheets with built-in column ordering

**NEW: Attribute-Based Column Ordering**:
```csharp
public class AddressEntity : VisitEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }
    
    [JsonPropertyName("address")]
    [SheetOrder(SheetsConfig.HeaderNames.Address)]  // References header constants
    public string Address { get; set; } = "";
    
    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal Distance { get; set; }
    
    // Inherits ordered properties from base classes:
    // - AmountEntity: Pay, Tips, Bonus, Total, Cash
    // - VisitEntity: Trips, FirstTrip, LastTrip
}
```

**Inheritance-Aware Ordering**: The system respects inheritance hierarchy:
```csharp
// Base class (AmountEntity)
[SheetOrder(SheetsConfig.HeaderNames.Pay)]
public decimal? Pay { get; set; }

[SheetOrder(SheetsConfig.HeaderNames.Tips)]  
public decimal? Tip { get; set; }

// Middle class (VisitEntity)
[SheetOrder(SheetsConfig.HeaderNames.Trips)]
public int Trips { get; set; }

// Derived class (AddressEntity)
[SheetOrder(SheetsConfig.HeaderNames.Address)]
public string Address { get; set; } = "";

// Resulting column order: Pay, Tips, Bonus, Total, Cash, Trips, FirstTrip, LastTrip, Address, Distance
```

**Characteristics**:
- Inherit from base classes (`AmountEntity`, `VisitEntity`) for shared properties
- Include `RowId` for Google Sheets row mapping
- Use JSON property names for serialization
- **NEW**: Use `SheetOrder` attributes to define column order without hardcoding numbers
- May contain more or fewer properties than sheet columns

### 3. Mapper Classes with Entity-Driven Ordering
**Purpose**: Translate between entities and Google Sheets data structures with automatic column ordering

**NEW: Entity-Driven Configuration**:
```csharp
public static class AddressMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.AddressSheet;
        
        // NEW: Apply entity-driven column ordering
        var entityColumnOrder = EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>(
            sheet.Headers,
            null // Fallback order for unmapped headers
        );
        
        // Validate that entity attributes reference valid header constants
        var allAvailableHeaders = typeof(SheetsConfig.HeaderNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!
            .ToList();
            
        var validationErrors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<AddressEntity>(allAvailableHeaders);
        
        if (validationErrors.Any())
        {
            throw new InvalidOperationException($"AddressEntity has invalid header mappings: {string.Join(", ", validationErrors)}");
        }
        
        sheet.Headers.UpdateColumns(); // Still needed for formula references
        
        // ... configure formulas as before
    }
}
```

**Core Methods**:
```csharp
public static class AddressMapper
{
    // Google Sheets data → Entity objects (unchanged)
    public static List<AddressEntity> MapFromRangeData(IList<IList<object>> values)
    
    // Entity objects → Google Sheets data (unchanged)
    public static IList<IList<object?>> MapToRangeData(List<AddressEntity> entities, IList<object> headers)
    
    // NEW: Configure sheet formulas and formatting with entity-driven ordering
    public static SheetModel GetSheet()
}
```

### 4. Header Management (`HeaderHelpers`)
**Purpose**: Parse and extract data from Google Sheets rows (unchanged)

**Key Functions**:
```csharp
// Parse header row into column index dictionary
Dictionary<int, string> ParserHeader(IList<object> sheetHeader)

// Extract typed values using column names
string GetStringValue(string columnName, IList<object> values, Dictionary<int, string> headers)
int GetIntValue(string columnName, IList<object> values, Dictionary<int, string> headers)
decimal GetDecimalValue(string columnName, IList<object> values, Dictionary<int, string> headers)
```

### 5. NEW: Entity Column Order Helper
**Purpose**: Extract and validate column ordering from entity attributes

**Key Functions**:
```csharp
// Extract column order from entity SheetOrder attributes
List<string> GetColumnOrderFromEntity<T>(List<SheetCellModel>? sheetHeaders, List<string>? headerOrder)

// Validate that entity attributes reference valid header constants
List<string> ValidateEntityHeaderMapping<T>(IEnumerable<string> availableHeaders)
```

## Current Workflow

### Sheet Creation Process (Enhanced)

1. **Define Entity with Ordering**: Create entity class with `SheetOrder` attributes
2. **Define Configuration**: Add static `SheetModel` to `SheetsConfig` (optional reordering)
3. **Implement Mapper**: Create mapper with entity-driven column ordering
4. **Configure Formulas**: Set up formulas and formatting in `GetSheet()` method

### Column Order Management (Improved)

**NEW: Entity-Driven Approach**:
```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.AddressSheet;
    
    // NEW: Entity defines the column order
    EntityColumnOrderHelper.GetColumnOrderFromEntity<AddressEntity>(sheet.Headers);
    
    sheet.Headers.UpdateColumns(); // Sets column indexes A, B, C, etc.
    
    // Now safe to reference columns by index for formulas
    var keyRange = sheet.GetLocalRange(HeaderEnum.ADDRESS.GetDescription());
}
```

**Benefits of New Approach**:
- **Single Source of Truth**: Entity properties define column order
- **References Constants**: Uses `SheetsConfig.HeaderNames` for consistency
- **Inheritance Support**: Handles complex entity hierarchies automatically
- **Validation**: Ensures entity attributes reference valid headers
- **No Hardcoding**: Eliminates order numbers or column letters
- **Readable**: Column order is visible in entity definition

**Why This Matters**:
- Enables formula references without hardcoding column letters
- Maintains correct order when mapping to/from Google Sheets
- Supports complex formulas that reference other columns
- Allows dynamic column insertion without breaking downstream references
- **NEW**: Column order is now defined in entities and automatically applied

### Data Flow (Enhanced)

**Reading Data** (unchanged):
```
Google Sheets Range Data → HeaderHelpers.ParserHeader() → Entity Properties
```

**Writing Data** (unchanged):
```
Entity Properties → MapToRangeData/MapToRowData → Google Sheets Batch Update
```

**Sheet Generation** (enhanced):
```
Entity SheetOrder Attributes → EntityColumnOrderHelper → SheetsConfig → Mapper.GetSheet() → Formula Configuration → Google Sheets Creation
```

## Complexity Areas

### 1. Formula Management (unchanged)
**Current Challenge**: Complex cross-sheet formulas with manual configuration
```csharp
case HeaderEnum.TOTAL_TIME_ACTIVE:
    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotalTimeActive(
        dateRange, 
        HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription(), 
        sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription()), 
        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
        keyRange, 
        tripSheet.GetRange(HeaderEnum.DURATION.GetDescription())
    );
```

### 2. Header Order Dependencies (Improved)
**OLD Issue**: Manual column order management across all mappers
- Must call `UpdateColumns()` at the right time
- Changes to column order require updates in multiple mappers
- Complex cross-sheet references need precise column coordination

**NEW Solution**: Entity-driven ordering with validation
- Column order defined in entity using `SheetOrder` attributes
- Automatic validation ensures entity references valid headers
- Inheritance hierarchy automatically handled
- Still need `UpdateColumns()` for formula column references

### 3. Shared Pattern Duplication (unchanged)
**Current Problem**: Similar aggregation patterns repeated across mappers
```csharp
// Similar code appears in AddressMapper, NameMapper, PlaceMapper, etc.
MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, tripSheet, tripKeyRange, useShiftTotals: false);
MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);
```

### 4. Sheet Type Categories (unchanged)
**Writeable Sheets**: `Trips`, `Shifts`, `Expenses`, `Setup`
**Aggregation Sheets**: `Addresses`, `Names`, `Places`, `Services`, `Daily`, `Weekly`, etc.

Different categories have different patterns but share common configuration complexity.

## Potential Improvements

### 1. ✅ IMPLEMENTED: Attribute-Based Column Ordering
**Benefits**:
- Automatic column order from entity definition
- Reduces mapper configuration complexity
- Single source of truth for column order
- References header constants for consistency
- Supports inheritance hierarchies

**Current Status**: Implemented with `SheetOrderAttribute` and `EntityColumnOrderHelper`

### 2. Configuration-Driven Sheet Templates
Create sheet template system with common patterns:
```csharp
public static SheetTemplate AggregationSheetTemplate => new()
{
    BaseHeaders = ["Key", "Trips", "Pay", "Tips", "Bonus", "Total", "Cash"],
    CommonFormulas = [
        new FormulaPattern { Header = "Total", Type = FormulaType.Sum, Sources = ["Pay", "Tips", "Bonus"] },
        new FormulaPattern { Header = "AmountPerTrip", Type = FormulaType.Division, Numerator = "Total", Denominator = "Trips" }
    ],
    AggregationSource = SheetType.Trips
};
```

### 3. Fluent Sheet Builder API
Simplify mapper configuration:
```csharp
public static SheetModel GetSheet()
{
    return SheetBuilder.Create(SheetsConfig.AddressSheet)
        .WithEntityOrdering<AddressEntity>()  // NEW: Use entity ordering
        .WithAggregationFrom(TripMapper.GetSheet())
        .GroupBy(HeaderEnum.ADDRESS_START, HeaderEnum.ADDRESS_END)
        .WithCommonFinancialFormulas()
        .WithCommonRatioFormulas()
        .WithVisitDateTracking()
        .Build();
}
```

### 4. Validation and Convention System
Add compile-time validation for common issues:
```csharp
// Analyzer rules
[SheetValidation]
public static SheetModel GetSheet()  // Validates UpdateColumns() called, formula references valid, etc.
```

## Recommendations

### Short-term Improvements
1. **✅ DONE: Standardize Entity Ordering**: Implement `SheetOrderAttribute` system
2. **Expand Attribute Usage**: Add `SheetOrder` attributes to all entity classes
3. **Improve Documentation**: Add more detailed column notes and formula explanations
4. **Validation Enhancement**: Expand `HeaderHelpers.CheckSheetHeaders()` with better error reporting

### Medium-term Improvements
1. **Template System**: Implement configuration-driven templates for common sheet types
2. **Formula Validation**: Add compile-time checking for formula references
3. **Abstraction Layer**: Create higher-level APIs for common mapping scenarios
4. **Fluent Builder**: Implement fluent API using entity ordering

### Long-term Improvements
1. **Enhanced Attributes**: Add format, validation, and formula attributes to entities
2. **Code Generation**: Consider generating mappers from entity definitions
3. **Performance Optimization**: Cache sheet configurations and optimize batch operations

## NEW: Entity-Driven Column Ordering Usage

### Adding SheetOrder Attributes to Entities

```csharp
public class TripEntity : AmountEntity
{
    [JsonPropertyName("date")]
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";
    
    [JsonPropertyName("service")]
    [SheetOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";
    
    // Inherits ordered financial properties from AmountEntity
    // Final order: Pay, Tips, Bonus, Total, Cash, Date, Service, ...
}
```

### Using Entity Ordering in Mappers

```csharp
public static SheetModel GetSheet()
{
    var sheet = SheetsConfig.TripSheet;
    
    // Apply entity-driven ordering
    EntityColumnOrderHelper.GetColumnOrderFromEntity<TripEntity>(sheet.Headers);
    
    // Validate entity mappings
    var errors = EntityColumnOrderHelper.ValidateEntityHeaderMapping<TripEntity>(availableHeaders);
    if (errors.Any()) throw new InvalidOperationException(string.Join(", ", errors));
    
    sheet.Headers.UpdateColumns(); // Required for formula references
    
    // Configure formulas as usual...
}
```

### Benefits Achieved

1. **Maintainable Order**: Column order visible in entity definition
2. **Constant References**: Uses `SheetsConfig.HeaderNames` exclusively  
3. **Inheritance Support**: Base class properties ordered correctly
4. **Validation**: Compile-time checks for invalid header references
5. **No Magic Numbers**: Eliminates hardcoded column positions
6. **Flexible**: Can still override order in SheetsConfig if needed

## Summary

The **attribute-based column ordering system** successfully addresses the major complexity of manual column management while preserving the current system's power and flexibility. By using `SheetOrder` attributes that reference `SheetsConfig.HeaderNames` constants, the system now provides:

- **Single source of truth** for column order (in entities)
- **Strong typing** through header constant references
- **Automatic inheritance handling** for complex entity hierarchies
- **Validation** to ensure entity attributes reference valid headers
- **Backward compatibility** with existing formula and configuration systems

This approach eliminates the need for hardcoded order numbers while maintaining the explicit control that complex Google Sheets formulas require. The system handles the complexity well for its intended use cases, and the new entity-driven ordering significantly improves maintainability as the number of sheets and columns grows.

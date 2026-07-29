# Building Custom Packages

RaptorSheets.Core with the TypedField system is designed to be the foundation for domain-specific
packages — this is exactly how RaptorSheets.Gig and RaptorSheets.Stock are built. If you just want
to *use* one of those packages, see [Getting Started](GETTING-STARTED.md) instead; this doc is for
building your own on top of Core.

Two shapes, depending on what you're building:

- **A handful of independent sheets, no shared orchestration needed** — hand-roll a manager around
  `BaseEntityRepository<T>` (see [Basic custom package](#basic-custom-package)).
- **Several related sheets that need to self-heal, and report properties/tab names/layouts in a
  consistent way** (like Gig or Stock) — inherit `SheetManagerBase<TEntity>` instead (see
  [Multi-sheet domain managers](#multi-sheet-domain-managers-recommended)).

## Basic custom package

```csharp
// 1. Define your domain entities with ColumnAttribute
public class ProductEntity
{
    public int RowId { get; set; }
    
    [Column(SheetsConfig.HeaderNames.ProductName, FieldType.String)]
    public string Name { get; set; } = "";
    
    [Column(SheetsConfig.HeaderNames.Price, FieldType.Currency)]
    public decimal Price { get; set; }
    
    [Column(SheetsConfig.HeaderNames.LaunchDate, FieldType.DateTime, "M/d/yyyy")]
    public DateTime? LaunchDate { get; set; }
}

// 2. Create repository with automatic CRUD
public class ProductRepository : BaseEntityRepository<ProductEntity>
{
    public ProductRepository(IGoogleSheetService service) 
        : base(service, "Products", hasHeaderRow: true) { }
    
    public async Task<List<ProductEntity>> GetExpensiveProductsAsync()
    {
        var products = await GetAllAsync(); // Automatic conversion
        return products.Where(p => p.Price > 100m).ToList();
    }
}

// 3. Domain-specific manager
public class ProductManager
{
    private readonly ProductRepository _repository;
    
    public ProductManager(Dictionary<string, string> credentials, string spreadsheetId)
    {
        var service = new GoogleSheetService(credentials, spreadsheetId);
        _repository = new ProductRepository(service);
    }
    
    public async Task<List<ProductEntity>> GetProductCatalogAsync()
    {
        return await _repository.GetAllAsync(); // Full type conversion automatically
    }
}
```

## Multi-sheet domain managers (recommended)

For a package that manages several related sheets (like Gig or Stock), inherit
`SheetManagerBase<TEntity>` instead of hand-rolling a manager. You supply a
`SheetRegistry<TEntity>`, the canonical ordered sheet-name list, and one method describing how to
(re)create missing sheets — and you inherit `GetSheets`/`GetAllSheets` orchestration, sheet
properties, tab names, layouts, `InsertMissingColumns`, and missing-column auto-healing:

```csharp
// 1. A Sheets container holding your typed row collections, and a top-level SheetEntity built on
//    SheetEntityBase<TSheets> (Properties/Sheets/Messages come from Core). Row collections live
//    under Sheets rather than flat on SheetEntity, so a domain sheet can never collide with the
//    reserved Properties/Messages members.
public class CatalogSheets
{
    public List<ProductEntity> Products { get; set; } = [];
}

public class SheetEntity : SheetEntityBase<CatalogSheets>
{
}

// 2. A registry mapping each sheet name to its headers + row mapping (RegisterGeneric uses
//    GenericSheetMapper<T>; Register lets you plug a hand-rolled mapper)
public static class CatalogSheetHelpers
{
    public static SheetRegistry<SheetEntity> Registry { get; } = Build();

    private static SheetRegistry<SheetEntity> Build()
    {
        var registry = new SheetRegistry<SheetEntity>();
        registry.RegisterGeneric<SheetEntity, ProductEntity>(
            "Products", ProductMapper.GetSheet, (se, rows) => se.Sheets.Products = rows);
        return registry;
    }
}

// 3. A manager that is little more than "hand Core the registry + names + how to create sheets"
public class CatalogManager : SheetManagerBase<SheetEntity>
{
    public CatalogManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, CatalogSheetHelpers.Registry, ["Products"], logger) { }

    // The one required domain hook: restore sheets found missing during GetSheets self-heal.
    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
        => CreateSheets(missingIndexMap); // your own create logic

    // ...plus your domain write operations (CreateSheets, ChangeSheetData, DeleteSheets)
}
```

`GetSheets`, `GetAllSheets`, `GetSheetProperties`, `GetAllSheetTabNames`, `GetSheetLayout(s)`,
`InsertMissingColumns`, and `GetSpreadsheetTitle` all come from the base — no per-domain
re-implementation.

**See [RaptorSheets.Gig](../RaptorSheets.Gig/README.md) as a complete example of a specialized package built on the TypedField system.**

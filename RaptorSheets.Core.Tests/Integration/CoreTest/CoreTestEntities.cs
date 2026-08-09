using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Purpose-built, domain-agnostic schema for live-testing RaptorSheets.Core's own plumbing
/// (SheetManagerBase, ColumnInsertionHelper, SheetRegistry) against a real spreadsheet - not modeled
/// on any shipped domain (Gig/Job/Home/Stock). See GitHub issue #100.
/// </summary>
[ExcludeFromCodeCoverage]
public class ItemEntity : SheetRowEntityBase
{
    [Column("Name", isInput: true, order: 0)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Column("Category", isInput: true, order: 1)]
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [Column("Amount", isInput: true, formatType: Format.ACCOUNTING, note: "Enter the amount in USD", order: 2)]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [Column("Active", isInput: true, order: 3)]
    [JsonPropertyName("active")]
    public bool Active { get; set; }
}

/// <summary>
/// Independent of <see cref="ItemEntity"/> - exists so "delete one sheet, others survive" can be
/// tested without touching the Items/Summary dependency graph.
/// </summary>
[ExcludeFromCodeCoverage]
public class LogEntity : SheetRowEntityBase
{
    [Column("Date", isInput: true, formatType: Format.DATE, order: 0)]
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [Column("Description", isInput: true, order: 1)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
}

/// <summary>
/// Fully formula-driven, one row per distinct Items.Category - cross-references the Items sheet (see
/// CoreTestSheetDefinitions.SummarySheetDefinition.GetSheet()), the "calculated/child sheet" for
/// exercising SheetRegistry.GetDependents/RefreshDependentSheetsAsync. Category is a genuine key
/// column (SORT/UNIQUE pulled from Items, same as Stock's Account/Ticker sheets) - Total/Count's
/// ARRAYFORMULAs spread against THIS column's own populated rows, not an empty self-reference.
/// </summary>
[ExcludeFromCodeCoverage]
public class SummaryEntity : SheetRowEntityBase
{
    [Column("Category", isInput: false, order: 0)]
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [Column("Total", isInput: false, formatType: Format.ACCOUNTING, order: 1)]
    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [Column("Count", isInput: false, formatType: Format.NUMBER, order: 2)]
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class CoreTestSheets
{
    public List<ItemEntity> Items { get; set; } = [];
    public List<LogEntity> Log { get; set; } = [];
    public List<SummaryEntity> Summary { get; set; } = [];
}

public class CoreTestSheetEntity : SheetEntityBase<CoreTestSheets>
{
}

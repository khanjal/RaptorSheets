using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity : EntityBase
{
    public int RowId { get; set; }
    
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    
    [ColumnOrder(SheetsConfig.HeaderNames.Date)]
    public DateTime Date { get; set; }
    
    [ColumnOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = string.Empty;
    
    [ColumnOrder(SheetsConfig.HeaderNames.Amount)]
    public decimal Amount { get; set; }
    
    [ColumnOrder(SheetsConfig.HeaderNames.Category)]
    public string Category { get; set; } = string.Empty;
    
    [ColumnOrder(SheetsConfig.HeaderNames.Description)]
    public string Description { get; set; } = string.Empty;
}

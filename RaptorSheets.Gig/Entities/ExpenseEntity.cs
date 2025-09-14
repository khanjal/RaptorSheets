using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity
{
    public int RowId { get; set; }
    
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public DateTime Date { get; set; }
    
    [SheetOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = string.Empty;
    
    [SheetOrder(SheetsConfig.HeaderNames.Amount)]
    public decimal Amount { get; set; }
    
    [SheetOrder(SheetsConfig.HeaderNames.Category)]
    public string Category { get; set; } = string.Empty;
    
    [SheetOrder(SheetsConfig.HeaderNames.Description)]
    public string Description { get; set; } = string.Empty;
}

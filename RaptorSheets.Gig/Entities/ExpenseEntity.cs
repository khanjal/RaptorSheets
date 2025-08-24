using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity
{
    public int RowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
}

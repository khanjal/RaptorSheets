using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity : SheetRowEntityBase
{
    [Header(SheetsConfig.HeaderNames.Date)]
    [Input]
    [Note(ColumnNotes.DateFormat)]
    [Format(FormatEnum.DATE)]
    public string Date { get; set; } = string.Empty;
    
    [Header(SheetsConfig.HeaderNames.Name)]
    [Input]
    public string Name { get; set; } = string.Empty;
    
    [Header(SheetsConfig.HeaderNames.Amount)]
    [Input]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal Amount { get; set; }
    
    [Header(SheetsConfig.HeaderNames.Category)]
    [Input]
    [Validation(SheetsConfig.ValidationNames.RangeSelf)]
    public string Category { get; set; } = string.Empty;
    
    [Header(SheetsConfig.HeaderNames.Description)]
    [Input]
    public string Description { get; set; } = string.Empty;
}

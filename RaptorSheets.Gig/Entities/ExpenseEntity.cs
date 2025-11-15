using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Date, FormatEnum.DATE, isInput: true, note: ColumnNotes.DateFormat)]
    public string Date { get; set; } = string.Empty;  // Changed from DateTime to string to match TripEntity/ShiftEntity pattern
    
    [Column(SheetsConfig.HeaderNames.Name, isInput: true)]
    public string Name { get; set; } = string.Empty;
    
    [Column(SheetsConfig.HeaderNames.Amount, isInput: true)]
    public decimal Amount { get; set; }
    
    [Column(SheetsConfig.HeaderNames.Category, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSelf)]
    public string Category { get; set; } = string.Empty;
    
    [Column(SheetsConfig.HeaderNames.Description, isInput: true)]
    public string Description { get; set; } = string.Empty;
}

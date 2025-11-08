using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class ExpenseEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.DateTime, isInput: true, note: ColumnNotes.DateFormat)]
    public DateTime Date { get; set; }
    
    [Column(SheetsConfig.HeaderNames.Name, FieldTypeEnum.String, isInput: true)]
    public string Name { get; set; } = string.Empty;
    
    [Column(SheetsConfig.HeaderNames.Amount, FieldTypeEnum.Currency, isInput: true)]
    public decimal Amount { get; set; }
    
    [Column(SheetsConfig.HeaderNames.Category, FieldTypeEnum.String, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSelf)]
    public string Category { get; set; } = string.Empty;
    
    [Column(SheetsConfig.HeaderNames.Description, FieldTypeEnum.String, isInput: true)]
    public string Description { get; set; } = string.Empty;
}

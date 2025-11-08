using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Expense mapper for Expenses sheet configuration.
/// For data mapping operations, use GenericSheetMapper&lt;ExpenseEntity&gt; directly.
/// </summary>
public static class ExpenseMapper
{
    /// <summary>
    /// Gets the configured Expenses sheet with validations and formatting.
    /// </summary>
    public static SheetModel GetSheet()
    {
        // Expenses sheet has minimal formulas - mostly just validations and formatting
        // which are handled by ColumnAttribute on the entity
        return GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet);
    }
}
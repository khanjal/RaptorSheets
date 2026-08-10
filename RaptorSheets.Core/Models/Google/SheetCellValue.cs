namespace RaptorSheets.Core.Models.Google;

/// <summary>
/// Wraps a Google Sheets cell's display text (FormattedValue) alongside its computed numeric
/// value (EffectiveValue.NumberValue), for cells read via a structure-aware request
/// (<see cref="RaptorSheets.Core.Helpers.SheetHelpers.GetSheetValues"/>, IncludeGridData=true)
/// where both are available. Numeric FieldTypes read <see cref="EffectiveNumber"/> directly
/// instead of re-parsing display text, sidestepping locale/currency-symbol/accounting-dash text
/// quirks entirely (GitHub issue #80). Every other FieldType is unaffected: <see cref="ToString"/>
/// returns <see cref="FormattedValue"/>, so the existing text-based readers (dates, strings,
/// booleans) keep working unchanged against this type without needing to know it exists.
/// </summary>
public class SheetCellValue
{
    public string? FormattedValue { get; }
    public double EffectiveNumber { get; }

    public SheetCellValue(string? formattedValue, double effectiveNumber)
    {
        FormattedValue = formattedValue;
        EffectiveNumber = effectiveNumber;
    }

    public override string ToString() => FormattedValue ?? "";
}

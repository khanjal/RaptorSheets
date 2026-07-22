using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Builds the two <see cref="DataValidationRule"/> shapes every domain's own
/// <c>GetDataValidation(Validation)</c> dispatches to (Gig's <c>GigSheetHelpers</c>, Stock's
/// <c>StockSheetHelpers</c>): a plain boolean checkbox, and a dropdown restricted to a range of
/// existing values elsewhere in the spreadsheet (another sheet's column, or a range on the same
/// sheet). Which <see cref="RaptorSheets.Core.Enums.Validation"/> members map to which rule -
/// and which sheet/range backs a given ONE_OF_RANGE rule - stays domain-specific.
/// </summary>
public static class GoogleValidationHelper
{
    public static DataValidationRule CreateBooleanRule()
    {
        return new DataValidationRule
        {
            Condition = new BooleanCondition { Type = "BOOLEAN" }
        };
    }

    /// <param name="range">The range formula text, without the leading '=' (e.g. "Names!A2:A").</param>
    public static DataValidationRule CreateOneOfRangeRule(string range)
    {
        return new DataValidationRule
        {
            Condition = new BooleanCondition
            {
                Type = "ONE_OF_RANGE",
                Values = [new ConditionValue { UserEnteredValue = $"={range}" }]
            },
            ShowCustomUi = true,
            Strict = false
        };
    }
}

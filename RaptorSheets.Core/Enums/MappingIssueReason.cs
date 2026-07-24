namespace RaptorSheets.Core.Enums;

/// <summary>
/// Why a cell's value could not be carried onto its target property during
/// <see cref="Mappers.GenericSheetMapper{T}.MapFromRangeData(System.Collections.Generic.IList{System.Collections.Generic.IList{object}}, string?, out System.Collections.Generic.List{Models.MappingIssue}, bool)"/>.
/// Diagnostic only - see <see cref="Models.MappingIssue"/> remarks.
/// </summary>
public enum MappingIssueReason
{
    /// <summary>
    /// The cell had non-blank text, but it could not be converted to the property's numeric type
    /// (Integer, Number, Currency, Accounting, or Percentage). The property was left at its type
    /// default rather than the read failing.
    /// </summary>
    CouldNotParseValue
}

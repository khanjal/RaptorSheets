using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public const string DateFormat = "Format: YYYY-MM-DD";
    public const string ReplacementMonths = "How many months between filter replacements.\n\nUsed with Filter Date to calculate Next Filter.";
    public const string NextFilter = "Calculated: Filter Date + Rpl. Mth.";
    public const string SquareFeet = "Calculated: L x W.";
    public const string Retired = "Check when this contact is no longer used.";
}

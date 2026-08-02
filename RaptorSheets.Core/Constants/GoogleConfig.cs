using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

[ExcludeFromCodeCoverage]
public static class GoogleConfig
{
    public static string AppName => "Raptor Sheets Engine";
    public static string ColumnLetters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static int DefaultColumnCount => 26;
    public static string Range => "A1:ZZZ10000000";
    public static string KeyRange => "A1:A";
    public static string ValidationRange => "A2:A";
    public static string HeaderRange => "1:1";

    // Header row plus the first data row - format/validation are written starting at the first data
    // row (see GoogleRequestHelpers.GenerateRepeatCellRequest), never on the header cell itself, so a
    // structure-only read needs both rows even though it only cares about column definitions.
    public static string HeaderStructureRange => "1:2";
    public static string RowRange => "A:A";
}

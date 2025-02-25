using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

[ExcludeFromCodeCoverage]
public static class GoogleConfig
{
    public static string AppName => "Raptor Sheets Engine";
    public static string ColumnLetters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static int DefaultColumnCount => 26;
    public static string FieldsUpdate => "*";
    public static string Range => "A1:ZZZ10000000";
    public static string KeyRange => "A1:A";
    public static string ValidationRange => "A2:A";
    public static string HeaderRange => "1:1";
    public static string RowRange => "A:A";
}

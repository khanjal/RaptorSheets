using System.Diagnostics.CodeAnalysis;

namespace GigRaptorLib.Constants;

[ExcludeFromCodeCoverage]
public static class GoogleConfig
{
    public static string AppendDimensionType => "COLUMNS";
    public static string AppName => "GigLogger Library";
    public static int DefaultColumnCount => 26;
    public static string FieldsUpdate => "*";
    public static string Range => "A1:ZZZ10000000";
}

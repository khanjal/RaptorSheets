using System.Diagnostics.CodeAnalysis;

namespace RLE.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ProtectionWarnings
{
    public static string ColumnWarning => "Editing this column will cause a #REF error.";
    public static string HeaderWarning => "Editing the header could cause a #REF error or break sheet references.";
    public static string SheetWarning => "Editing this sheet will cause a #REF error.";
}

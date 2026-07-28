using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>One sheet within a domain's Sheets container (e.g. GigSheets.Trips) - Name is the real
/// spreadsheet tab name, which is not always the same as the C# property it's reflected from.</summary>
public record SheetDescriptor(string Name, Type RowType, PropertyInfo ListProperty)
{
    public IList GetRows(object sheetsContainer) => (IList)ListProperty.GetValue(sheetsContainer)!;
}

/// <summary>
/// Reflects over a domain's Sheets container to discover its sheets, so the sample never hardcodes
/// a domain's schema - column-level reflection is already public on RaptorSheets.Core as
/// <see cref="RaptorSheets.Core.Utilities.TypedFieldUtils.GetColumnProperties(Type)"/>, reused
/// as-is by EntityGrid.
/// </summary>
public static class SheetMetadata
{
    /// <summary>
    /// A Sheets-container property's own name isn't always its real spreadsheet tab name - C#
    /// identifiers can't hold spaces or "&amp;", so Job's InterviewTypes property is really the
    /// "Interview Types" tab, and Home's DoorsWindows property is really "Doors &amp; Windows".
    /// sheetNamesType is the domain's own Constants.SheetsConfig.SheetNames class, whose field names
    /// match the Sheets-container property names 1:1 by convention - its value is the real tab name.
    /// Falls back to the property name itself if no matching constant is found, or if
    /// sheetNamesType is null (Stock has no such class - it names sheets via an enum instead, and
    /// its sheet names are all single words identical to their property names anyway).
    /// </summary>
    public static IReadOnlyList<SheetDescriptor> GetSheetDescriptors(Type sheetsType, Type? sheetNamesType)
    {
        var realNames = sheetNamesType?
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue()!)
            ?? [];

        return sheetsType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            .Where(p => typeof(SheetRowEntityBase).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]))
            .Select(p => new SheetDescriptor(
                realNames.GetValueOrDefault(p.Name, p.Name),
                p.PropertyType.GetGenericArguments()[0],
                p))
            .ToList();
    }
}

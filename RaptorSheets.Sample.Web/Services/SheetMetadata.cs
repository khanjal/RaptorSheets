using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// One sheet within a domain's Sheets container (e.g. GigSheets.Trips) - the property name doubles
/// as the sheet's tab name by convention across every domain (Trips property, "Trips" tab).
/// </summary>
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
    public static IReadOnlyList<SheetDescriptor> GetSheetDescriptors(Type sheetsType) =>
        sheetsType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            .Where(p => typeof(SheetRowEntityBase).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]))
            .Select(p => new SheetDescriptor(p.Name, p.PropertyType.GetGenericArguments()[0], p))
            .ToList();
}

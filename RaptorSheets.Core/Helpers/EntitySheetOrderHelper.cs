using System.Reflection;
using RaptorSheets.Core.Attributes;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for extracting sheet ordering from entity SheetOrder attributes.
/// This provides a centralized way to maintain sheet tab order consistency.
/// </summary>
public static class EntitySheetOrderHelper
{
    /// <summary>
    /// Extracts sheet order from an entity type based on SheetOrder attributes.
    /// Returns sheet names in order specified by the Order property.
    /// Unordered sheets (Order = -1) are placed first in property declaration order.
    /// Ordered sheets are then inserted at their specific positions relative to the start of the final list.
    /// Out-of-range ordered sheets are placed at the end.
    /// </summary>
    /// <typeparam name="T">The entity type to extract sheet order from</typeparam>
    /// <returns>List of sheet names in entity-defined order</returns>
    public static List<string> GetSheetOrderFromEntity<T>()
    {
        var entityType = typeof(T);
        var sheetInfos = new List<(int Order, string SheetName, int PropertyIndex)>();

        // Get all properties with SheetOrder attributes
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        for (int i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var sheetOrderAttr = property.GetCustomAttribute<SheetOrderAttribute>();
            if (sheetOrderAttr != null)
            {
                // Validate order is not negative (except -1 which means unordered)
                if (sheetOrderAttr.Order < -1)
                {
                    throw new InvalidOperationException($"Invalid order value {sheetOrderAttr.Order} for property '{property.Name}' in entity '{entityType.Name}'. Order must be -1 (unordered) or >= 0.");
                }

                sheetInfos.Add((sheetOrderAttr.Order, sheetOrderAttr.SheetName, i));
            }
        }

        if (sheetInfos.Count == 0)
            return new List<string>();

        // Separate unordered (-1) and ordered sheets
        var unorderedSheets = sheetInfos
            .Where(info => info.Order == -1)
            .OrderBy(info => info.PropertyIndex) // Property declaration order
            .Select(info => info.SheetName)
            .ToList();

        var orderedSheets = sheetInfos
            .Where(info => info.Order != -1)
            .OrderBy(info => info.Order)
            .ToList();

        // Start with unordered sheets
        var result = new List<string>(unorderedSheets);
        var unorderedCount = unorderedSheets.Count;

        // Insert ordered sheets at their specific positions (adjusted for unordered sheets at start)
        foreach (var orderedSheet in orderedSheets)
        {
            var targetPosition = unorderedCount + orderedSheet.Order;

            // If position is within current list bounds, insert at that position
            if (targetPosition <= result.Count)
            {
                result.Insert(targetPosition, orderedSheet.SheetName);
            }
            else
            {
                // Position is beyond list bounds, add at the end
                result.Add(orderedSheet.SheetName);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that all SheetOrder attributes in an entity reference valid sheet names
    /// and have unique order values (excluding -1 for optional orders).
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="availableSheets">List of valid sheet names (e.g., from SheetsConfig.SheetNames)</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntitySheetMapping<T>(IEnumerable<string> availableSheets)
    {
        var entityType = typeof(T);
        // Filter out null values from available sheets
        var availableSheetsSet = availableSheets.Where(s => s != null).ToHashSet();
        var errors = new List<string>();
        var usedOrders = new HashSet<int>();
        var usedSheetNames = new HashSet<string>();

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var sheetOrderAttr = property.GetCustomAttribute<SheetOrderAttribute>();
            if (sheetOrderAttr != null)
            {
                // Validate sheet name exists
                if (string.IsNullOrEmpty(sheetOrderAttr.SheetName) || !availableSheetsSet.Contains(sheetOrderAttr.SheetName!))
                {
                    errors.Add($"Property '{property.Name}' in entity '{entityType.Name}' " +
                              $"references sheet '{sheetOrderAttr.SheetName ?? "null"}' which is not available in " +
                              $"SheetsConfig.SheetNames. Please add this sheet to the constants or " +
                              $"update the SheetOrder attribute.");
                }

                // Validate order is unique (excluding -1 for optional orders)
                if (sheetOrderAttr.Order != -1)
                {
                    if (usedOrders.Contains(sheetOrderAttr.Order))
                    {
                        errors.Add($"Order {sheetOrderAttr.Order} is used multiple times in entity '{entityType.Name}'. " +
                                  $"Each SheetOrder attribute must have a unique Order value.");
                    }
                    else
                    {
                        usedOrders.Add(sheetOrderAttr.Order);
                    }
                }

                // Validate sheet name is unique
                if (usedSheetNames.Contains(sheetOrderAttr.SheetName!))
                {
                    errors.Add($"Sheet name '{sheetOrderAttr.SheetName}' is used multiple times in entity '{entityType.Name}'. " +
                              $"Each SheetOrder attribute must reference a unique sheet name.");
                }
                else
                {
                    usedSheetNames.Add(sheetOrderAttr.SheetName!);
                }
            }
        }

        return errors;
    }
}
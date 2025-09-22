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
    /// </summary>
    /// <typeparam name="T">The entity type to extract sheet order from</typeparam>
    /// <returns>List of sheet names in entity-defined order</returns>
    public static List<string> GetSheetOrderFromEntity<T>()
    {
        var entityType = typeof(T);
        var sheetInfos = new List<(int Order, string SheetName)>();

        // Get all properties with SheetOrder attributes
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            var sheetOrderAttr = property.GetCustomAttribute<SheetOrderAttribute>();
            if (sheetOrderAttr != null)
            {
                sheetInfos.Add((sheetOrderAttr.Order, sheetOrderAttr.SheetName));
            }
        }

        // Return sheet names in the order properties are declared
        return sheetInfos.Select(info => info.SheetName).ToList();
    }

    /// <summary>
    /// Validates that all SheetOrder attributes in an entity reference valid sheet names
    /// and have unique order values.
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="availableSheets">List of valid sheet names (e.g., from SheetsConfig.SheetNames)</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntitySheetMapping<T>(IEnumerable<string> availableSheets)
    {
        var entityType = typeof(T);
        var availableSheetsSet = availableSheets.ToHashSet();
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
                if (!availableSheetsSet.Contains(sheetOrderAttr.SheetName))
                {
                    errors.Add($"Property '{property.Name}' in entity '{entityType.Name}' " +
                              $"references sheet '{sheetOrderAttr.SheetName}' which is not available in " +
                              $"SheetsConfig.SheetNames. Please add this sheet to the constants or " +
                              $"update the SheetOrder attribute.");
                }

                // Validate order is unique
                if (usedOrders.Contains(sheetOrderAttr.Order))
                {
                    errors.Add($"Order {sheetOrderAttr.Order} is used multiple times in entity '{entityType.Name}'. " +
                              $"Each SheetOrder attribute must have a unique Order value.");
                }
                else
                {
                    usedOrders.Add(sheetOrderAttr.Order);
                }

                // Validate sheet name is unique
                if (usedSheetNames.Contains(sheetOrderAttr.SheetName))
                {
                    errors.Add($"Sheet name '{sheetOrderAttr.SheetName}' is used multiple times in entity '{entityType.Name}'. " +
                              $"Each SheetOrder attribute must reference a unique sheet name.");
                }
                else
                {
                    usedSheetNames.Add(sheetOrderAttr.SheetName);
                }
            }
        }

        return errors;
    }
}
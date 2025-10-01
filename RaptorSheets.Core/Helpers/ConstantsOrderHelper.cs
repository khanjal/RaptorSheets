using System.Reflection;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for extracting sheet ordering from constants classes.
/// Uses the declaration order of constants to determine sheet order.
/// </summary>
public static class ConstantsOrderHelper
{
    /// <summary>
    /// Extracts sheet order from a constants class based on field declaration order.
    /// </summary>
    /// <param name="constantsType">Type containing string constants (e.g., SheetsConfig.SheetNames)</param>
    /// <returns>List of constant values in declaration order</returns>
    public static List<string> GetOrderFromConstants(Type constantsType)
    {
        var fields = constantsType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .OrderBy(f => f.MetadataToken) // This preserves source declaration order
            .ToList();

        var result = new List<string>();
        foreach (var field in fields)
        {
            if (field.GetValue(null) is string value)
            {
                result.Add(value);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that all provided sheet names exist in the constants class.
    /// </summary>
    /// <param name="constantsType">Type containing string constants</param>
    /// <param name="sheetNames">Sheet names to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateSheetNames(Type constantsType, IEnumerable<string> sheetNames)
    {
        var validNames = GetOrderFromConstants(constantsType).ToHashSet();
        var errors = new List<string>();

        foreach (var sheetName in sheetNames)
        {
            if (!validNames.Contains(sheetName))
            {
                errors.Add($"Sheet name '{sheetName}' is not defined in {constantsType.Name}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets the index (order position) of a sheet name in the constants declaration.
    /// </summary>
    /// <param name="constantsType">Type containing string constants</param>
    /// <param name="sheetName">Sheet name to find</param>
    /// <returns>Zero-based index, or -1 if not found</returns>
    public static int GetSheetIndex(Type constantsType, string sheetName)
    {
        var orderedNames = GetOrderFromConstants(constantsType);
        return orderedNames.IndexOf(sheetName);
    }
}
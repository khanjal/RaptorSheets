using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Factories;

/// <summary>
/// Factory pattern for creating sheet models with consistent validation and error handling.
/// This helps standardize sheet creation across different domain packages.
/// </summary>
public static class SheetModelFactory
{
    /// <summary>
    /// Create a sheet model with validation
    /// </summary>
    /// <param name="sheetCreator">Function that creates the sheet model</param>
    /// <param name="sheetName">Name of the sheet being created for error reporting</param>
    /// <returns>Sheet model or null if creation fails</returns>
    public static SheetModel? CreateSheet(Func<SheetModel> sheetCreator, string sheetName)
    {
        try
        {
            var sheet = sheetCreator();
            
            // Basic validation
            if (string.IsNullOrWhiteSpace(sheet.Name))
            {
                throw new InvalidOperationException($"Sheet name cannot be empty for {sheetName}");
            }

            if (sheet.Headers == null || sheet.Headers.Count == 0)
            {
                throw new InvalidOperationException($"Sheet must have at least one header for {sheetName}");
            }

            return sheet;
        }
        catch (Exception ex)
        {
            // Log the error - in a real implementation you'd use proper logging
            Console.WriteLine($"Error creating sheet {sheetName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create multiple sheet models with batch validation
    /// </summary>
    public static Dictionary<string, SheetModel> CreateSheets(Dictionary<string, Func<SheetModel>> sheetCreators)
    {
        var sheets = new Dictionary<string, SheetModel>();

        foreach (var (sheetName, sheetCreator) in sheetCreators)
        {
            var sheet = CreateSheet(sheetCreator, sheetName);
            if (sheet != null)
            {
                sheets[sheetName] = sheet;
            }
        }

        return sheets;
    }

    /// <summary>
    /// Validate that a sheet model is properly configured
    /// </summary>
    public static List<string> ValidateSheetModel(SheetModel sheet)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sheet.Name))
            errors.Add("Sheet name cannot be empty");

        if (sheet.Headers == null || sheet.Headers.Count == 0)
            errors.Add("Sheet must have at least one header");

        // Check for duplicate header names
        if (sheet.Headers != null)
        {
            var duplicateHeaders = sheet.Headers
                .GroupBy(h => h.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateHeaders.Count > 0)
                errors.Add($"Duplicate header names found: {string.Join(", ", duplicateHeaders)}");
        }

        return errors;
    }
}
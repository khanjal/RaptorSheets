using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper methods for standardizing sheet configuration patterns across domain packages.
/// </summary>
public static class SheetConfigurationHelpers
{
    /// <summary>
    /// Configure a sheet model with headers using the standardized pattern:
    /// 1. Start from base configuration
    /// 2. Update column indexes
    /// 3. Apply header-specific configurations via action delegate
    /// </summary>
    /// <param name="baseSheet">Base sheet configuration from SheetsConfig</param>
    /// <param name="headerConfigurator">Action to configure individual headers</param>
    /// <returns>Configured sheet model</returns>
    public static SheetModel ConfigureSheet(SheetModel baseSheet, Action<SheetCellModel, int> headerConfigurator)
    {
        // Ensure column indexes are properly assigned
        baseSheet.Headers.UpdateColumns();

        // Apply header-specific configurations
        for (int i = 0; i < baseSheet.Headers.Count; i++)
        {
            headerConfigurator(baseSheet.Headers[i], i);
        }

        return baseSheet;
    }

    /// <summary>
    /// Create a formula column with automatic array formula wrapping
    /// </summary>
    public static string CreateArrayFormula(string headerText, string keyRange, string formula)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{headerText}\",ISBLANK({keyRange}), \"\",true,{formula}))";
    }

    /// <summary>
    /// Create a simple date-based column formula (DAY, MONTH, YEAR)
    /// </summary>
    public static string CreateDatePartFormula(string headerText, string dateRange, string datePart)
    {
        return $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{headerText}\",ISBLANK({dateRange}), \"\",true,{datePart}({dateRange})))";
    }

    /// <summary>
    /// Apply common formatting patterns based on header content
    /// </summary>
    public static void ApplyCommonFormats(SheetCellModel header, string headerName)
    {
        var lowerName = headerName.ToLowerInvariant();
        
        if (lowerName.Contains("date"))
            header.Format = FormatEnum.DATE;
        else if (lowerName.Contains("time") && !lowerName.Contains("active"))
            header.Format = lowerName.Contains("duration") || lowerName.Contains("total") ? FormatEnum.DURATION : FormatEnum.TIME;
        else if (lowerName.Contains("amount") || lowerName.Contains("pay") || lowerName.Contains("tip") || 
                 lowerName.Contains("bonus") || lowerName.Contains("cash") || lowerName.Contains("cost") ||
                 lowerName.Contains("total") || lowerName.Contains("return"))
            header.Format = FormatEnum.ACCOUNTING;
        else if (lowerName.Contains("distance") || lowerName.Contains("dist"))
            header.Format = FormatEnum.DISTANCE;
        else if (lowerName.Contains("trips") || lowerName.Contains("shares") || lowerName.Contains("number"))
            header.Format = FormatEnum.NUMBER;
        else if (lowerName.Contains("weekday"))
            header.Format = FormatEnum.WEEKDAY;
    }
}
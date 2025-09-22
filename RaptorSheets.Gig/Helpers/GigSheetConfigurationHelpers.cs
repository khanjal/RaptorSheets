using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Enums;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Gig-specific helper methods for standardizing sheet configuration patterns.
/// Provides type-safe header formatting based on HeaderEnum constants instead of string matching.
/// </summary>
public static class GigSheetConfigurationHelpers
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
    /// Apply common formatting patterns based on HeaderEnum values.
    /// This provides type safety and consistency compared to string-based matching.
    /// Only applies formatting for headers that match known HeaderEnum values.
    /// </summary>
    /// <param name="header">The sheet cell model to format</param>
    /// <param name="headerName">The header name to parse into HeaderEnum</param>
    public static void ApplyCommonFormats(SheetCellModel header, string headerName)
    {
        // Try to parse the header name into a HeaderEnum for type-safe formatting
        var headerEnum = headerName.GetValueFromName<HeaderEnum>();
        
        // Check if GetValueFromName found a real match by comparing with the original string
        // GetValueFromName returns the first enum value if no match is found
        var enumDescription = headerEnum.GetDescription();
        var isRealMatch = string.Equals(headerName, enumDescription, StringComparison.OrdinalIgnoreCase);
        
        if (!isRealMatch)
        {
            // No matching HeaderEnum found - leave header formatting unchanged
            return;
        }

        ApplyFormatsByHeaderEnum(header, headerEnum);
    }

    /// <summary>
    /// Apply formatting based on HeaderEnum values using a switch statement for optimal performance.
    /// </summary>
    /// <param name="header">The sheet cell model to format</param>
    /// <param name="headerEnum">The HeaderEnum value</param>
    public static void ApplyFormatsByHeaderEnum(SheetCellModel header, HeaderEnum headerEnum)
    {
        switch (headerEnum)
        {
            // Date formatting
            case HeaderEnum.DATE:
            case HeaderEnum.DATE_BEGIN:
            case HeaderEnum.DATE_END:
            case HeaderEnum.VISIT_FIRST:
            case HeaderEnum.VISIT_LAST:
                header.Format = FormatEnum.DATE;
                break;

            // Time formatting
            case HeaderEnum.TIME_START:
            case HeaderEnum.TIME_END:
                header.Format = FormatEnum.TIME;
                break;

            // Duration formatting
            case HeaderEnum.DURATION:
            case HeaderEnum.TIME_TOTAL:
            case HeaderEnum.TIME_ACTIVE:
            case HeaderEnum.TOTAL_TIME:
            case HeaderEnum.TOTAL_TIME_ACTIVE:
                header.Format = FormatEnum.DURATION;
                break;

            // Accounting/Money formatting
            case HeaderEnum.AMOUNT:
            case HeaderEnum.AMOUNT_CURRENT:
            case HeaderEnum.AMOUNT_PREVIOUS:
            case HeaderEnum.AMOUNT_PER_DAY:
            case HeaderEnum.AMOUNT_PER_DISTANCE:
            case HeaderEnum.AMOUNT_PER_PREVIOUS_DAY:
            case HeaderEnum.AMOUNT_PER_TIME:
            case HeaderEnum.AMOUNT_PER_TRIP:
            case HeaderEnum.PAY:
            case HeaderEnum.TIP:
            case HeaderEnum.TIPS:
            case HeaderEnum.BONUS:
            case HeaderEnum.CASH:
            case HeaderEnum.TOTAL:
            case HeaderEnum.TOTAL_BONUS:
            case HeaderEnum.TOTAL_CASH:
            case HeaderEnum.TOTAL_PAY:
            case HeaderEnum.TOTAL_TIPS:
            case HeaderEnum.TOTAL_GRAND:
            case HeaderEnum.AVERAGE:
                header.Format = FormatEnum.ACCOUNTING;
                break;

            // Distance formatting
            case HeaderEnum.DISTANCE:
            case HeaderEnum.TOTAL_DISTANCE:
                header.Format = FormatEnum.DISTANCE;
                break;

            // Number formatting
            case HeaderEnum.NUMBER:
            case HeaderEnum.ORDER_NUMBER:
            case HeaderEnum.TRIPS:
            case HeaderEnum.TRIPS_PER_DAY:
            case HeaderEnum.TRIPS_PER_HOUR:
            case HeaderEnum.TOTAL_TRIPS:
            case HeaderEnum.VISITS:
            case HeaderEnum.DAYS:
            case HeaderEnum.NUMBER_OF_DAYS:
            case HeaderEnum.DAYS_PER_VISIT:
            case HeaderEnum.DAYS_SINCE_VISIT:
            case HeaderEnum.ODOMETER_START:
            case HeaderEnum.ODOMETER_END:
                header.Format = FormatEnum.NUMBER;
                break;

            // Weekday formatting
            case HeaderEnum.WEEKDAY:
                header.Format = FormatEnum.WEEKDAY;
                break;

            // Text formatting (explicit for clarity, though DEFAULT would work)
            case HeaderEnum.ADDRESS:
            case HeaderEnum.ADDRESS_START:
            case HeaderEnum.ADDRESS_END:
            case HeaderEnum.NAME:
            case HeaderEnum.PLACE:
            case HeaderEnum.PICKUP:
            case HeaderEnum.DROPOFF:
            case HeaderEnum.REGION:
            case HeaderEnum.SERVICE:
            case HeaderEnum.TYPE:
            case HeaderEnum.CATEGORY:
            case HeaderEnum.DESCRIPTION:
            case HeaderEnum.NOTE:
            case HeaderEnum.UNIT_END:
                header.Format = FormatEnum.TEXT;
                break;

            // Default case for headers that don't need specific formatting
            default:
                header.Format = FormatEnum.DEFAULT;
                break;
        }
    }
}
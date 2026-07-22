using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Enums;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Gig-specific helper methods for standardizing sheet configuration patterns.
/// Provides type-safe header formatting based on Header constants instead of string matching.
/// </summary>
public static class GigSheetConfigurationHelpers
{
    /// <summary>
    /// Apply common formatting patterns based on Header values.
    /// This provides type safety and consistency compared to string-based matching.
    /// Only applies formatting for headers that match known Header values.
    /// </summary>
    /// <param name="header">The sheet cell model to format</param>
    /// <param name="headerName">The header name to parse into Header</param>
    public static void ApplyCommonFormats(SheetCellModel header, string headerName)
    {
        // Try to parse the header name into a Header for type-safe formatting
        var headerEnum = headerName.GetValueFromName<Header>();
        
        // Check if GetValueFromName found a real match by comparing with the original string
        // GetValueFromName returns the first enum value if no match is found
        var enumDescription = headerEnum.GetDescription();
        var isRealMatch = string.Equals(headerName, enumDescription, StringComparison.OrdinalIgnoreCase);
        
        if (!isRealMatch)
        {
            // No matching Header found - leave header formatting unchanged
            return;
        }

        ApplyFormatsByHeaderEnum(header, headerEnum);
    }

    /// <summary>
    /// Apply formatting based on Header values using a switch statement for optimal performance.
    /// </summary>
    /// <param name="header">The sheet cell model to format</param>
    /// <param name="headerEnum">The Header value</param>
    public static void ApplyFormatsByHeaderEnum(SheetCellModel header, Header headerEnum)
    {
        switch (headerEnum)
        {
            // Date formatting
            case Header.DATE:
            case Header.DATE_BEGIN:
            case Header.DATE_END:
            case Header.VISIT_FIRST:
            case Header.VISIT_LAST:
                header.Format = Format.DATE;
                break;

            // Time formatting
            case Header.TIME_START:
            case Header.TIME_END:
                header.Format = Format.TIME;
                break;

            // Duration formatting
            case Header.DURATION:
            case Header.TIME_TOTAL:
            case Header.TIME_ACTIVE:
            case Header.TOTAL_TIME:
            case Header.TOTAL_TIME_ACTIVE:
                header.Format = Format.DURATION;
                break;

            // Accounting/Money formatting
            case Header.AMOUNT:
            case Header.AMOUNT_CURRENT:
            case Header.AMOUNT_PREVIOUS:
            case Header.AMOUNT_PER_DAY:
            case Header.AMOUNT_PER_DISTANCE:
            case Header.AMOUNT_PER_PREVIOUS_DAY:
            case Header.AMOUNT_PER_TIME:
            case Header.AMOUNT_PER_TRIP:
            case Header.PAY:
            case Header.TIP:
            case Header.TIPS:
            case Header.BONUS:
            case Header.CASH:
            case Header.TOTAL:
            case Header.TOTAL_BONUS:
            case Header.TOTAL_CASH:
            case Header.TOTAL_PAY:
            case Header.TOTAL_TIPS:
            case Header.TOTAL_GRAND:
            case Header.AVERAGE:
                header.Format = Format.ACCOUNTING;
                break;

            // Distance formatting
            case Header.DISTANCE:
            case Header.TOTAL_DISTANCE:
                header.Format = Format.DISTANCE;
                break;

            // Number formatting
            case Header.NUMBER:
            case Header.ORDER_NUMBER:
            case Header.TRIPS:
            case Header.TRIPS_PER_DAY:
            case Header.TRIPS_PER_HOUR:
            case Header.TOTAL_TRIPS:
            case Header.VISITS:
            case Header.DAYS:
            case Header.NUMBER_OF_DAYS:
            case Header.DAYS_PER_VISIT:
            case Header.DAYS_SINCE_VISIT:
            case Header.ODOMETER_START:
            case Header.ODOMETER_END:
                header.Format = Format.NUMBER;
                break;

            // Weekday formatting
            case Header.WEEKDAY:
                header.Format = Format.WEEKDAY;
                break;

            // Text formatting (explicit for clarity, though DEFAULT would work)
            case Header.ADDRESS:
            case Header.ADDRESS_START:
            case Header.ADDRESS_END:
            case Header.NAME:
            case Header.PLACE:
            case Header.PICKUP:
            case Header.DROPOFF:
            case Header.REGION:
            case Header.SERVICE:
            case Header.TYPE:
            case Header.CATEGORY:
            case Header.DESCRIPTION:
            case Header.NOTE:
            case Header.UNIT_END:
                header.Format = Format.TEXT;
                break;

            // Default case for headers that don't need specific formatting
            default:
                header.Format = Format.DEFAULT;
                break;
        }
    }
}
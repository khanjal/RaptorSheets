using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Enums;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Common helper for configuring formula patterns that are shared across multiple mappers
/// Eliminates duplication by centralizing common formula configuration logic
/// </summary>
public static class MapperFormulaHelper
{
    /// <summary>
    /// Configure common aggregation headers with flexible source handling
    /// </summary>
    /// <param name="sheet">Target sheet to configure</param>
    /// <param name="keyRange">Key range in target sheet</param>
    /// <param name="sourceSheet">Source sheet to aggregate from</param>
    /// <param name="sourceKeyRange">Key range in source sheet</param>
    /// <param name="useShiftTotals">When true: use TOTAL_* columns (shift-level). When false: use base columns (trip-level). Default: false</param>
    /// <param name="countTrips">When true: count trip occurrences instead of summing. When false: sum values. Default: false</param>
    public static void ConfigureCommonAggregationHeaders(
        SheetModel sheet,
        string keyRange,
        SheetModel sourceSheet,
        string sourceKeyRange,
        bool useShiftTotals = false,
        bool countTrips = false)
    {
        string GetRange(Header baseEnum, Header totalEnum)
            => useShiftTotals ? sourceSheet.GetRange(totalEnum.GetDescription()) : sourceSheet.GetRange(baseEnum.GetDescription());

        foreach (var header in sheet.Headers)
        {
            var headerEnum = header.Name.GetValueFromName<Header>();
            switch (headerEnum)
            {
                case Header.PAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, Header.PAY.GetDescription(), sourceKeyRange, GetRange(Header.PAY, Header.TOTAL_PAY));
                    header.Format = Format.ACCOUNTING;
                    break;

                case Header.TIPS:
                case Header.TIP:
                    ConfigureTipsHeader(header, headerEnum, keyRange, sourceSheet, sourceKeyRange, useShiftTotals);
                    break;

                case Header.BONUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, Header.BONUS.GetDescription(), sourceKeyRange, GetRange(Header.BONUS, Header.TOTAL_BONUS));
                    header.Format = Format.ACCOUNTING;
                    break;

                case Header.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
                        keyRange, Header.TOTAL.GetDescription(),
                        sheet.GetLocalRange(Header.PAY.GetDescription()),
                        sheet.GetLocalRange(Header.TIPS.GetDescription()),
                        sheet.GetLocalRange(Header.BONUS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;

                case Header.CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, Header.CASH.GetDescription(), sourceKeyRange, GetRange(Header.CASH, Header.TOTAL_CASH));
                    header.Format = Format.ACCOUNTING;
                    break;

                case Header.TRIPS:
                    ConfigureTripsHeader(header, keyRange, sourceSheet, sourceKeyRange, useShiftTotals, countTrips);
                    break;

                case Header.DAYS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                        keyRange, Header.DAYS.GetDescription(), sourceKeyRange);
                    header.Format = Format.NUMBER;
                    break;

                case Header.DISTANCE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, Header.DISTANCE.GetDescription(), sourceKeyRange, GetRange(Header.DISTANCE, Header.TOTAL_DISTANCE));
                    header.Format = Format.DISTANCE;
                    break;

                case Header.TIME_TOTAL:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, Header.TIME_TOTAL.GetDescription(), sourceKeyRange, GetRange(Header.TIME_TOTAL, Header.TOTAL_TIME));
                    header.Format = Format.DURATION;
                    break;
            }
        }
    }

    private static void ConfigureTipsHeader(SheetCellModel header, Header headerEnum, string keyRange, SheetModel sourceSheet, string sourceKeyRange, bool useShiftTotals)
    {
        var tipsRange = useShiftTotals
            ? sourceSheet.GetRange(Header.TOTAL_TIPS.GetDescription())
            : sourceSheet.GetRange(Header.TIPS.GetDescription());
        var headerName = headerEnum == Header.TIP
            ? Header.TIP.GetDescription()
            : Header.TIPS.GetDescription();

        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, headerName, sourceKeyRange, tipsRange);
        header.Format = Format.ACCOUNTING;
    }

    private static void ConfigureTripsHeader(SheetCellModel header, string keyRange, SheetModel sourceSheet, string sourceKeyRange, bool useShiftTotals, bool countTrips)
    {
        if (countTrips)
        {
            // Scenario 3: Count trip occurrences (trip-level data with no TRIPS column)
            header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                keyRange, Header.TRIPS.GetDescription(), sourceKeyRange);
            header.Format = Format.NUMBER;
            return;
        }

        // Sum scenarios: shift-level TOTAL_TRIPS column, or daily/monthly aggregated TRIPS column
        var sourceRange = useShiftTotals
            ? sourceSheet.GetRange(Header.TOTAL_TRIPS.GetDescription())
            : sourceSheet.GetRange(Header.TRIPS.GetDescription());

        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
            keyRange, Header.TRIPS.GetDescription(), sourceKeyRange, sourceRange);
        header.Format = Format.NUMBER;
    }

    /// <summary>
    /// Configure common calculated ratio headers (Amount per Trip, Amount per Distance, etc.)
    /// </summary>
    public static void ConfigureCommonRatioHeaders(SheetModel sheet, string keyRange)
    {
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();
            
            switch (headerEnum)
            {
                case Header.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, Header.AMOUNT_PER_TRIP.GetDescription(), 
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(Header.TRIPS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, Header.AMOUNT_PER_DISTANCE.GetDescription(), 
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(Header.DISTANCE.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, Header.AMOUNT_PER_TIME.GetDescription(), 
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(Header.TIME_TOTAL.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDay(keyRange, Header.AMOUNT_PER_DAY.GetDescription(), 
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(Header.DAYS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
            }
        });
    }

    /// <summary>
    /// Configure unique value headers for simple dropdowns (Service, Region, etc.)
    /// </summary>
    public static void ConfigureUniqueValueHeader(SheetCellModel header, string sourceRange)
    {
        var headerEnum = header.Name.GetValueFromName<Header>();
        var headerName = headerEnum.GetDescription();
        
        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(headerName, sourceRange);
        
        // Set validation if this is a dropdown source
        switch (headerEnum)
        {
            case Header.SERVICE:
                header.Validation = Validation.RANGE_SERVICE.GetDescription();
                break;
            case Header.REGION:
                header.Validation = Validation.RANGE_REGION.GetDescription();
                break;
            case Header.PLACE:
                header.Validation = Validation.RANGE_PLACE.GetDescription();
                break;
        }
    }

    /// <summary>
    /// Configure combined unique value headers for dual-source dropdowns (Start+End Address, etc.)
    /// Uses filtered version to exclude empty values by default
    /// </summary>
    public static void ConfigureCombinedUniqueValueHeader(SheetCellModel header, string range1, string range2)
    {
        var headerEnum = header.Name.GetValueFromName<Header>();
        var headerName = headerEnum.GetDescription();
        
        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombinedFiltered(headerName, range1, range2);
    }

    /// <summary>
    /// Configure dual count headers that count from multiple ranges (like Address Trips counting both start and end)
    /// </summary>
    public static void ConfigureDualCountHeader(SheetCellModel header, string keyRange, string range1, string range2)
    {
        var headerEnum = header.Name.GetValueFromName<Header>();
        var headerName = headerEnum.GetDescription();

        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaDualCountIf(keyRange, headerName, range1, range2);
        header.Format = Format.NUMBER;
    }

    /// <summary>
    /// Configure the Day/Month/Year date-part headers shared by any sheet keyed off a Date column
    /// (Trips, Shifts). No-op for any other header - callers should only reach this for a header
    /// they've already matched as DAY/MONTH/YEAR.
    /// </summary>
    public static void ConfigureDatePartHeader(SheetCellModel header, Header headerEnum, string dateRange)
    {
        switch (headerEnum)
        {
            case Header.DAY:
                header.Formula = GoogleFormulaBuilder.BuildArrayFormulaDay(dateRange, Header.DAY.GetDescription(), dateRange);
                break;
            case Header.MONTH:
                header.Formula = GoogleFormulaBuilder.BuildArrayFormulaMonth(dateRange, Header.MONTH.GetDescription(), dateRange);
                break;
            case Header.YEAR:
                header.Formula = GoogleFormulaBuilder.BuildArrayFormulaYear(dateRange, Header.YEAR.GetDescription(), dateRange);
                break;
        }
    }
}
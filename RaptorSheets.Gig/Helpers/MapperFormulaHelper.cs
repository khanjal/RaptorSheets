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
        void SetHeaderFormulaAndFormat(SheetCellModel header, string formula, Format format)
        {
            header.Formula = formula;
            header.Format = format;
        }

        string GetRange(Header baseEnum, Header totalEnum)
            => useShiftTotals ? sourceSheet.GetRange(totalEnum.GetDescription()) : sourceSheet.GetRange(baseEnum.GetDescription());

        foreach (var header in sheet.Headers)
        {
            var headerEnum = header.Name.GetValueFromName<Header>();
            switch (headerEnum)
            {
                case Header.PAY:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, Header.PAY.GetDescription(), sourceKeyRange, GetRange(Header.PAY, Header.TOTAL_PAY)),
                        Format.ACCOUNTING);
                    break;

                case Header.TIPS:
                case Header.TIP:
                    var tipsRange = useShiftTotals
                        ? sourceSheet.GetRange(Header.TOTAL_TIPS.GetDescription())
                        : sourceSheet.GetRange(Header.TIPS.GetDescription());
                    var headerName = headerEnum == Header.TIP
                        ? Header.TIP.GetDescription()
                        : Header.TIPS.GetDescription();
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, headerName, sourceKeyRange, tipsRange),
                        Format.ACCOUNTING);
                    break;

                case Header.BONUS:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, Header.BONUS.GetDescription(), sourceKeyRange, GetRange(Header.BONUS, Header.TOTAL_BONUS)),
                        Format.ACCOUNTING);
                    break;

                case Header.TOTAL:
                    SetHeaderFormulaAndFormat(
                        header,
                        GigFormulaBuilder.BuildArrayFormulaTotal(
                            keyRange, Header.TOTAL.GetDescription(),
                            sheet.GetLocalRange(Header.PAY.GetDescription()),
                            sheet.GetLocalRange(Header.TIPS.GetDescription()),
                            sheet.GetLocalRange(Header.BONUS.GetDescription())),
                        Format.ACCOUNTING);
                    break;

                case Header.CASH:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, Header.CASH.GetDescription(), sourceKeyRange, GetRange(Header.CASH, Header.TOTAL_CASH)),
                        Format.ACCOUNTING);
                    break;

                case Header.TRIPS:
                    if (countTrips)
                    {
                        // Scenario 3: Count trip occurrences (trip-level data with no TRIPS column)
                        SetHeaderFormulaAndFormat(
                            header,
                            GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                                keyRange, Header.TRIPS.GetDescription(), sourceKeyRange),
                            Format.NUMBER);
                    }
                    else
                    {
                        // Sum scenarios: check if using shift totals or daily/monthly data
                        if (useShiftTotals)
                        {
                            // Scenario 1: Sum shift-level TOTAL_TRIPS column
                            SetHeaderFormulaAndFormat(
                                header,
                                GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                                    keyRange, Header.TRIPS.GetDescription(), sourceKeyRange, sourceSheet.GetRange(Header.TOTAL_TRIPS.GetDescription())),
                                Format.NUMBER);
                        }
                        else
                        {
                            // Scenario 2: Sum TRIPS column (daily/monthly aggregated data)
                            SetHeaderFormulaAndFormat(
                                header,
                                GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                                    keyRange, Header.TRIPS.GetDescription(), sourceKeyRange, sourceSheet.GetRange(Header.TRIPS.GetDescription())),
                                Format.NUMBER);
                        }
                    }
                    break;

                case Header.DAYS:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                            keyRange, Header.DAYS.GetDescription(), sourceKeyRange),
                        Format.NUMBER);
                    break;

                case Header.DISTANCE:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, Header.DISTANCE.GetDescription(), sourceKeyRange, GetRange(Header.DISTANCE, Header.TOTAL_DISTANCE)),
                        Format.DISTANCE);
                    break;

                case Header.TIME_TOTAL:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, Header.TIME_TOTAL.GetDescription(), sourceKeyRange, GetRange(Header.TIME_TOTAL, Header.TOTAL_TIME)),
                        Format.DURATION);
                    break;
            }
        }
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
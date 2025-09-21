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
    public static void ConfigureCommonAggregationHeaders(SheetModel sheet, string keyRange, SheetModel sourceSheet, string sourceKeyRange, bool useShiftTotals = true)
    {
        void SetHeaderFormulaAndFormat(SheetCellModel header, string formula, FormatEnum format)
        {
            header.Formula = formula;
            header.Format = format;
        }

        string GetRange(HeaderEnum baseEnum, HeaderEnum totalEnum)
            => useShiftTotals ? sourceSheet.GetRange(totalEnum.GetDescription()) : sourceSheet.GetRange(baseEnum.GetDescription());

        foreach (var header in sheet.Headers)
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
            switch (headerEnum)
            {
                case HeaderEnum.PAY:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, HeaderEnum.PAY.GetDescription(), sourceKeyRange, GetRange(HeaderEnum.PAY, HeaderEnum.TOTAL_PAY)),
                        FormatEnum.ACCOUNTING);
                    break;

                case HeaderEnum.TIPS:
                case HeaderEnum.TIP:
                    var tipsRange = useShiftTotals
                        ? sourceSheet.GetRange(HeaderEnum.TOTAL_TIPS.GetDescription())
                        : sourceSheet.GetRange(HeaderEnum.TIPS.GetDescription());
                    var headerName = headerEnum == HeaderEnum.TIP
                        ? HeaderEnum.TIP.GetDescription()
                        : HeaderEnum.TIPS.GetDescription();
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, headerName, sourceKeyRange, tipsRange),
                        FormatEnum.ACCOUNTING);
                    break;

                case HeaderEnum.BONUS:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, HeaderEnum.BONUS.GetDescription(), sourceKeyRange, GetRange(HeaderEnum.BONUS, HeaderEnum.TOTAL_BONUS)),
                        FormatEnum.ACCOUNTING);
                    break;

                case HeaderEnum.TOTAL:
                    SetHeaderFormulaAndFormat(
                        header,
                        GigFormulaBuilder.BuildArrayFormulaTotal(
                            keyRange, HeaderEnum.TOTAL.GetDescription(),
                            sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()),
                            sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()),
                            sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription())),
                        FormatEnum.ACCOUNTING);
                    break;

                case HeaderEnum.CASH:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, HeaderEnum.CASH.GetDescription(), sourceKeyRange, GetRange(HeaderEnum.CASH, HeaderEnum.TOTAL_CASH)),
                        FormatEnum.ACCOUNTING);
                    break;

                case HeaderEnum.TRIPS:
                    if (useShiftTotals)
                    {
                        SetHeaderFormulaAndFormat(
                            header,
                            GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                                keyRange, HeaderEnum.TRIPS.GetDescription(), sourceKeyRange, sourceSheet.GetRange(HeaderEnum.TOTAL_TRIPS.GetDescription())),
                            FormatEnum.NUMBER);
                    }
                    else
                    {
                        SetHeaderFormulaAndFormat(
                            header,
                            GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                                keyRange, HeaderEnum.TRIPS.GetDescription(), sourceKeyRange),
                            FormatEnum.NUMBER);
                    }
                    break;

                case HeaderEnum.DAYS:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                            keyRange, HeaderEnum.DAYS.GetDescription(), sourceKeyRange),
                        FormatEnum.NUMBER);
                    break;

                case HeaderEnum.DISTANCE:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, HeaderEnum.DISTANCE.GetDescription(), sourceKeyRange, GetRange(HeaderEnum.DISTANCE, HeaderEnum.TOTAL_DISTANCE)),
                        FormatEnum.DISTANCE);
                    break;

                case HeaderEnum.TIME_TOTAL:
                    SetHeaderFormulaAndFormat(
                        header,
                        GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                            keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), sourceKeyRange, GetRange(HeaderEnum.TIME_TOTAL, HeaderEnum.TOTAL_TIME)),
                        FormatEnum.DURATION);
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
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, HeaderEnum.AMOUNT_PER_TIME.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDay(keyRange, HeaderEnum.AMOUNT_PER_DAY.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
            }
        });
    }

    /// <summary>
    /// Configure unique value headers for simple dropdowns (Service, Region, etc.)
    /// </summary>
    public static void ConfigureUniqueValueHeader(SheetCellModel header, string sourceRange)
    {
        var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
        var headerName = headerEnum.GetDescription();
        
        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUnique(headerName, sourceRange);
        
        // Set validation if this is a dropdown source
        switch (headerEnum)
        {
            case HeaderEnum.SERVICE:
                header.Validation = ValidationEnum.RANGE_SERVICE.GetDescription();
                break;
            case HeaderEnum.REGION:
                header.Validation = ValidationEnum.RANGE_REGION.GetDescription();
                break;
            case HeaderEnum.PLACE:
                header.Validation = ValidationEnum.RANGE_PLACE.GetDescription();
                break;
        }
    }

    /// <summary>
    /// Configure combined unique value headers for dual-source dropdowns (Start+End Address, etc.)
    /// </summary>
    public static void ConfigureCombinedUniqueValueHeader(SheetCellModel header, string range1, string range2)
    {
        var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
        var headerName = headerEnum.GetDescription();
        
        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(headerName, range1, range2);
    }

    /// <summary>
    /// Configure dual count headers that count from multiple ranges (like Address Trips counting both start and end)
    /// </summary>
    public static void ConfigureDualCountHeader(SheetCellModel header, string keyRange, string range1, string range2)
    {
        var headerEnum = header.Name.GetValueFromName<HeaderEnum>();
        var headerName = headerEnum.GetDescription();
        
        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaDualCountIf(keyRange, headerName, range1, range2);
        header.Format = FormatEnum.NUMBER;
    }
}
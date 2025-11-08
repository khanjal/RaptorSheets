using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Shift mapper for Shifts sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;ShiftEntity&gt; directly.
/// </summary>
public static class ShiftMapper
{
    /// <summary>
    /// Gets the configured Shifts sheet with formulas, validations, and formatting.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ShiftEntity>.GetSheet(
            SheetsConfig.ShiftSheet,
            ConfigureShiftFormulas
        );
    }

    /// <summary>
    /// Configures formulas specific to the Shifts sheet.
    /// Notes, validations, and formatting are handled by ColumnAttribute on the entity.
    /// This method only adds formulas that can't be defined at the entity level.
    /// </summary>
    private static void ConfigureShiftFormulas(SheetModel sheet)
    {
        var tripSheet = TripMapper.GetSheet();
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
        var keyRange = sheet.GetLocalRange(HeaderEnum.KEY.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.KEY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftKey(
                        dateRange, 
                        HeaderEnum.KEY.GetDescription(), 
                        dateRange, 
                        sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_TIME_ACTIVE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotalTimeActive(
                        dateRange, 
                        HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TIME_ACTIVE.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange, 
                        tripSheet.GetRange(HeaderEnum.DURATION.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotalTimeWithOmit(
                        dateRange, 
                        HeaderEnum.TOTAL_TIME.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TIME_OMIT.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TIME_ACTIVE.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_TRIPS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalTrips(
                        dateRange, 
                        HeaderEnum.TOTAL_TRIPS.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange);
                    break;
                case HeaderEnum.TOTAL_PAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange, 
                        HeaderEnum.TOTAL_PAY.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange, 
                        tripSheet.GetRange(HeaderEnum.PAY.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_TIPS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange, 
                        HeaderEnum.TOTAL_TIPS.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange, 
                        tripSheet.GetRange(HeaderEnum.TIPS.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_BONUS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange, 
                        HeaderEnum.TOTAL_BONUS.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange, 
                        tripSheet.GetRange(HeaderEnum.BONUS.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_GRAND:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
                        dateRange, 
                        HeaderEnum.TOTAL_GRAND.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_PAY.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TIPS.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_BONUS.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange, 
                        HeaderEnum.TOTAL_CASH.GetDescription(), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.CASH.GetDescription()));
                    break;
                case HeaderEnum.TOTAL_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange, 
                        HeaderEnum.TOTAL_DISTANCE.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()), 
                        tripSheet.GetRange(HeaderEnum.KEY.GetDescription()), 
                        keyRange, 
                        tripSheet.GetRange(HeaderEnum.DISTANCE.GetDescription()));
                    break;
                case HeaderEnum.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(
                        dateRange, 
                        HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription()));
                    break;
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange, 
                        HeaderEnum.AMOUNT_PER_TIME.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription()));
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                        dateRange, 
                        HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_GRAND.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_DISTANCE.GetDescription()));
                    break;
                case HeaderEnum.TRIPS_PER_HOUR:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange, 
                        HeaderEnum.TRIPS_PER_HOUR.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TRIPS.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL_TIME.GetDescription()));
                    break;
                case HeaderEnum.DAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaDay(
                        dateRange, 
                        HeaderEnum.DAY.GetDescription(), 
                        dateRange);
                    break;
                case HeaderEnum.MONTH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaMonth(
                        dateRange, 
                        HeaderEnum.MONTH.GetDescription(), 
                        dateRange);
                    break;
                case HeaderEnum.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaYear(
                        dateRange, 
                        HeaderEnum.YEAR.GetDescription(), 
                        dateRange);
                    break;
                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
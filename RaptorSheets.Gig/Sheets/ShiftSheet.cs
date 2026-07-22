using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Shift sheet definition - layout and formulas for the Shifts sheet.
/// For data mapping operations, use GenericSheetMapper&lt;ShiftEntity&gt; directly.
/// </summary>
public static class ShiftSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Shifts,
        TabColor = SheetColor.RED,
        CellColor = SheetColor.LIGHT_RED,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ShiftEntity>()
    };

    /// <summary>
    /// Gets the configured Shifts sheet with formulas, validations, and formatting.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ShiftEntity>.GetSheet(
            BaseSheet,
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
        var tripSheet = TripSheet.GetSheet();
        var dateRange = sheet.GetLocalRange(Header.DATE.GetDescription());
        var keyRange = sheet.GetLocalRange(Header.KEY.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.KEY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftKey(
                        dateRange,
                        Header.KEY.GetDescription(),
                        dateRange,
                        sheet.GetLocalRange(Header.SERVICE.GetDescription()),
                        sheet.GetLocalRange(Header.NUMBER.GetDescription()));
                    break;
                case Header.TOTAL_TIME_ACTIVE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotalTimeActive(
                        dateRange,
                        Header.TOTAL_TIME_ACTIVE.GetDescription(),
                        sheet.GetLocalRange(Header.TIME_ACTIVE.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange,
                        tripSheet.GetRange(Header.DURATION.GetDescription()));
                    break;
                case Header.TOTAL_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotalTimeWithOmit(
                        dateRange,
                        Header.TOTAL_TIME.GetDescription(),
                        sheet.GetLocalRange(Header.TIME_OMIT.GetDescription()),
                        sheet.GetLocalRange(Header.TIME_TOTAL.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_TIME_ACTIVE.GetDescription()));
                    break;
                case Header.TOTAL_TRIPS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalTrips(
                        dateRange,
                        Header.TOTAL_TRIPS.GetDescription(),
                        sheet.GetLocalRange(Header.TRIPS.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange);
                    break;
                case Header.TOTAL_PAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange,
                        Header.TOTAL_PAY.GetDescription(),
                        sheet.GetLocalRange(Header.PAY.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange,
                        tripSheet.GetRange(Header.PAY.GetDescription()));
                    break;
                case Header.TOTAL_TIPS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange,
                        Header.TOTAL_TIPS.GetDescription(),
                        sheet.GetLocalRange(Header.TIPS.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange,
                        tripSheet.GetRange(Header.TIPS.GetDescription()));
                    break;
                case Header.TOTAL_BONUS:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange,
                        Header.TOTAL_BONUS.GetDescription(),
                        sheet.GetLocalRange(Header.BONUS.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange,
                        tripSheet.GetRange(Header.BONUS.GetDescription()));
                    break;
                case Header.TOTAL_GRAND:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
                        dateRange,
                        Header.TOTAL_GRAND.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL_PAY.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_TIPS.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_BONUS.GetDescription()));
                    break;
                case Header.TOTAL_CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(
                        keyRange,
                        Header.TOTAL_CASH.GetDescription(),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        tripSheet.GetRange(Header.CASH.GetDescription()));
                    break;
                case Header.TOTAL_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(
                        dateRange,
                        Header.TOTAL_DISTANCE.GetDescription(),
                        sheet.GetLocalRange(Header.DISTANCE.GetDescription()),
                        tripSheet.GetRange(Header.KEY.GetDescription()),
                        keyRange,
                        tripSheet.GetRange(Header.DISTANCE.GetDescription()));
                    break;
                case Header.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(
                        dateRange,
                        Header.AMOUNT_PER_TRIP.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL_GRAND.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_TRIPS.GetDescription()));
                    break;
                case Header.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange,
                        Header.AMOUNT_PER_TIME.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL_GRAND.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_TIME.GetDescription()));
                    break;
                case Header.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                        dateRange,
                        Header.AMOUNT_PER_DISTANCE.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL_GRAND.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_DISTANCE.GetDescription()));
                    break;
                case Header.TRIPS_PER_HOUR:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange,
                        Header.TRIPS_PER_HOUR.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL_TRIPS.GetDescription()),
                        sheet.GetLocalRange(Header.TOTAL_TIME.GetDescription()));
                    break;
                case Header.DAY:
                case Header.MONTH:
                case Header.YEAR:
                    MapperFormulaHelper.ConfigureDatePartHeader(header, headerEnum, dateRange);
                    break;
                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}

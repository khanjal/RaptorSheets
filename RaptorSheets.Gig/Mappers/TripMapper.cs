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
/// Trip mapper for Trips sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;TripEntity&gt; directly.
/// </summary>
public static class TripMapper
{
    /// <summary>
    /// Gets the configured Trips sheet with formulas, validations, and formatting.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<TripEntity>.GetSheet(
            SheetsConfig.TripSheet,
            ConfigureTripFormulas
        );
    }

    /// <summary>
    /// Configures formulas specific to the Trips sheet.
    /// Notes, validations, and formatting are handled by ColumnAttribute on the entity.
    /// This method only adds formulas that can't be defined at the entity level.
    /// </summary>
    private static void ConfigureTripFormulas(SheetModel sheet)
    {
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
                        dateRange, 
                        HeaderEnum.TOTAL.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                    break;
                case HeaderEnum.KEY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTripKey(
                        dateRange, 
                        HeaderEnum.KEY.GetDescription(), 
                        dateRange, 
                        sheet.GetLocalRange(HeaderEnum.SERVICE.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.EXCLUDE.GetDescription()));
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
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange, 
                        HeaderEnum.AMOUNT_PER_TIME.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.DURATION.GetDescription()));
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                        dateRange, 
                        HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), 
                        sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                    break;
                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
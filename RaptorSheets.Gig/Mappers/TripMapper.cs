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
/// Trip mapper for configuring the Trips sheet with formulas, validations, and formatting.
/// This mapper leverages the GenericSheetMapper for entity-driven configuration.
/// </summary>
public static class TripMapper
{
    /// <summary>
    /// Retrieves the configured Trips sheet.
    /// Includes formulas, validations, and formatting specific to the Trips sheet.
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
    /// This method handles formulas that cannot be defined at the entity level.
    /// </summary>
    /// <param name="sheet">The Trips sheet model to configure.</param>
    private static void ConfigureTripFormulas(SheetModel sheet)
    {
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.TOTAL:
                    // Formula to calculate the total amount, including pay, tips, and bonus.
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
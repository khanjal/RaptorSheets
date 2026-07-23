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
/// Trip sheet definition - layout, formulas, validations, and formatting for the Trips sheet.
/// Leverages GenericSheetMapper for entity-driven configuration.
/// </summary>
public static class TripSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Trips,
        TabColor = SheetColor.DARK_YELLOW,
        CellColor = SheetColor.LIGHT_YELLOW,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TripEntity>()
    };

    /// <summary>
    /// Retrieves the configured Trips sheet.
    /// Includes formulas, validations, and formatting specific to the Trips sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<TripEntity>.GetSheet(
            BaseSheet,
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
        var dateRange = sheet.GetLocalRange(Header.DATE.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.TOTAL:
                    // Formula to calculate the total amount, including pay, tips, and bonus.
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(
                        dateRange,
                        Header.TOTAL.GetDescription(),
                        sheet.GetLocalRange(Header.PAY.GetDescription()),
                        sheet.GetLocalRange(Header.TIPS.GetDescription()),
                        sheet.GetLocalRange(Header.BONUS.GetDescription()));
                    break;
                case Header.KEY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTripKey(
                        dateRange,
                        Header.KEY.GetDescription(),
                        dateRange,
                        sheet.GetLocalRange(Header.SERVICE.GetDescription()),
                        sheet.GetLocalRange(Header.NUMBER.GetDescription()),
                        sheet.GetLocalRange(Header.EXCLUDE.GetDescription()));
                    break;
                case Header.DAY:
                case Header.MONTH:
                case Header.YEAR:
                    MapperFormulaHelper.ConfigureDatePartHeader(header, headerEnum, dateRange);
                    break;
                case Header.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(
                        dateRange,
                        Header.AMOUNT_PER_TIME.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()),
                        sheet.GetLocalRange(Header.DURATION.GetDescription()));
                    break;
                case Header.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                        dateRange,
                        Header.AMOUNT_PER_DISTANCE.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()),
                        sheet.GetLocalRange(Header.DISTANCE.GetDescription()));
                    break;
                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}

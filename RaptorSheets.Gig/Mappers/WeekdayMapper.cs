using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Weekday mapper for configuring the Weekday sheet with formulas, validations, and formatting.
/// This mapper is designed for weekday-specific data aggregation and calculations.
/// </summary>
public static class WeekdayMapper
{
    /// <summary>
    /// Retrieves the configured Weekday sheet.
    /// Includes formulas, validations, and formatting specific to the Weekday sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.WeekdaySheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.DAY.GetDescription());
        var dailyDayRange = dailySheet.GetRange(Header.DAY.GetDescription());
        var dailyDateToTotalRange = dailySheet.GetRangeBetweenColumns(Header.DATE.GetDescription(), Header.TOTAL.GetDescription());

        // Configure specific headers unique to WeekdayMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.DAY:
                    // Formula to generate unique weekday numbers from the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(Header.DAY.GetDescription(), dailySheet.GetRange(Header.DAY.GetDescription(), 2));
                    header.Format = Format.NUMBER;
                    break;
                case Header.WEEKDAY:
                    // Formula to generate weekday text based on day range.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(keyRange, Header.WEEKDAY.GetDescription(), keyRange, 1);
                    break;
                case Header.TRIPS:
                    // Formula to sum trips by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.TRIPS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.TRIPS.GetDescription()));
                    header.Format = Format.NUMBER;
                    break;
                case Header.DAYS:
                    // Formula to count days by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, Header.DAYS.GetDescription(), 
                        dailyDayRange);
                    header.Format = Format.NUMBER;
                    break;
                case Header.PAY:
                    // Formula to sum pay by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.PAY.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.PAY.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.TIPS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.TIPS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.TIPS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.BONUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.BONUS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.BONUS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, Header.TOTAL.GetDescription(), sheet.GetLocalRange(Header.PAY.GetDescription()), sheet.GetLocalRange(Header.TIPS.GetDescription()), sheet.GetLocalRange(Header.BONUS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.CASH.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.CASH.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, Header.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()), sheet.GetLocalRange(Header.TRIPS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.DISTANCE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.DISTANCE.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.DISTANCE.GetDescription()));
                    header.Format = Format.DISTANCE;
                    break;
                case Header.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, Header.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()), sheet.GetLocalRange(Header.DISTANCE.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.TIME_TOTAL:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.TIME_TOTAL.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(Header.TIME_TOTAL.GetDescription()));
                    header.Format = Format.DURATION;
                    break;
                case Header.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, Header.AMOUNT_PER_TIME.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()), sheet.GetLocalRange(Header.TIME_TOTAL.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDay(keyRange, Header.AMOUNT_PER_DAY.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()), sheet.GetLocalRange(Header.DAYS.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_CURRENT:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayAmount(keyRange, dailyDateToTotalRange, 0, Header.AMOUNT_CURRENT.GetDescription());
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PREVIOUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayAmount(keyRange, dailyDateToTotalRange, -7, Header.AMOUNT_PREVIOUS.GetDescription());
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.AMOUNT_PER_PREVIOUS_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaPreviousDayAverage(
                        keyRange,
                        Header.AMOUNT_PER_PREVIOUS_DAY.GetDescription(),
                        sheet.GetLocalRange(Header.TOTAL.GetDescription()),
                        sheet.GetLocalRange(Header.AMOUNT_PREVIOUS.GetDescription()),
                        sheet.GetLocalRange(Header.DAYS.GetDescription())
                    );
                    header.Format = Format.ACCOUNTING;
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}


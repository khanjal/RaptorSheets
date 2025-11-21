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
        var keyRange = sheet.GetLocalRange(HeaderEnum.DAY.GetDescription());
        var dailyDayRange = dailySheet.GetRange(HeaderEnum.DAY.GetDescription());
        var dailyDateToTotalRange = dailySheet.GetRangeBetweenColumns(HeaderEnum.DATE.GetDescription(), HeaderEnum.TOTAL.GetDescription());

        // Configure specific headers unique to WeekdayMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.DAY:
                    // Formula to generate unique weekday numbers from the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(HeaderEnum.DAY.GetDescription(), dailySheet.GetRange(HeaderEnum.DAY.GetDescription(), 2));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.WEEKDAY:
                    // Formula to generate weekday text based on day range.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(keyRange, HeaderEnum.WEEKDAY.GetDescription(), keyRange, 1);
                    break;
                case HeaderEnum.TRIPS:
                    // Formula to sum trips by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.TRIPS.GetDescription()));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.DAYS:
                    // Formula to count days by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.GetDescription(), 
                        dailyDayRange);
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.PAY:
                    // Formula to sum pay by weekday number using the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.PAY.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TIPS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.TIPS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.BONUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.BONUS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TOTAL:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.CASH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.CASH.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_TRIP:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.DISTANCE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.DISTANCE.GetDescription()));
                    header.Format = FormatEnum.DISTANCE;
                    break;
                case HeaderEnum.AMOUNT_PER_DISTANCE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.TIME_TOTAL:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                    header.Format = FormatEnum.DURATION;
                    break;
                case HeaderEnum.AMOUNT_PER_TIME:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, HeaderEnum.AMOUNT_PER_TIME.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDay(keyRange, HeaderEnum.AMOUNT_PER_DAY.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_CURRENT:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayAmount(keyRange, dailyDateToTotalRange, 0, HeaderEnum.AMOUNT_CURRENT.GetDescription());
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PREVIOUS:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayAmount(keyRange, dailyDateToTotalRange, -7, HeaderEnum.AMOUNT_PREVIOUS.GetDescription());
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.AMOUNT_PER_PREVIOUS_DAY:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaPreviousDayAverage(
                        keyRange,
                        HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(),
                        sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()),
                        sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS.GetDescription()),
                        sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())
                    );
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}


using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class WeekdayMapper
{
    public static List<WeekdayEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var weekdays = new List<WeekdayEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (var value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            // Console.Write(JsonSerializer.Serialize(value));
            WeekdayEntity weekday = new()
            {
                RowId = id,
                Day = HeaderHelpers.GetIntValue(HeaderEnum.DAY.GetDescription(), value, headers),
                Weekday = HeaderHelpers.GetStringValue(HeaderEnum.WEEKDAY.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                Days = HeaderHelpers.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
                DailyAverage = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                PreviousDailyAverage = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(), value, headers),
                CurrentAmount = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_CURRENT.GetDescription(), value, headers),
                PreviousAmount = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PREVIOUS.GetDescription(), value, headers),
            };

            weekdays.Add(weekday);
        }
        return weekdays;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.WeekdaySheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.DAY.GetDescription());
        var dailyDayRange = dailySheet.GetRange(HeaderEnum.DAY.GetDescription(), 2);
        var dailyDateToTotalRange = dailySheet.GetRangeBetweenColumns(HeaderEnum.DATE.GetDescription(), HeaderEnum.TOTAL.GetDescription());

        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.DAY:
                    // Use filtered unique formula for weekday numbers from Daily sheet
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.DAY.GetDescription(), dailyDayRange);
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.WEEKDAY:
                    // Use weekday text formula based on day range
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(keyRange, HeaderEnum.WEEKDAY.GetDescription(), keyRange, 1);
                    break;
                case HeaderEnum.TRIPS:
                    // Sum trips by weekday number using the existing DAY column from Daily sheet
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), 
                        dailyDayRange, 
                        dailySheet.GetRange(HeaderEnum.TRIPS.GetDescription()));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.DAYS:
                    // Count days by weekday number using the existing DAY column from Daily sheet
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.GetDescription(), 
                        dailyDayRange);
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.PAY:
                    // Sum pay by weekday number using the existing DAY column from Daily sheet
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
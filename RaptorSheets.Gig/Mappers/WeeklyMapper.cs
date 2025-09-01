using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers
{
    public static class WeeklyMapper
    {
        public static List<WeeklyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var weeklyList = new List<WeeklyEntity>();
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

                WeeklyEntity weekly = new()
                {
                    RowId = id,
                    Week = HeaderHelpers.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Days = HeaderHelpers.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
                    Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    AmountPerTrip = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                    Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    AmountPerDistance = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                    Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                    AmountPerTime = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                    Average = HeaderHelpers.GetDecimalValue(HeaderEnum.AVERAGE.GetDescription(), value, headers),
                    AmountPerDay = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                    Number = HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), value, headers),
                    Year = HeaderHelpers.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers),
                    Begin = HeaderHelpers.GetDateValue(HeaderEnum.DATE_BEGIN.GetDescription(), value, headers),
                    End = HeaderHelpers.GetDateValue(HeaderEnum.DATE_END.GetDescription(), value, headers),
                };

                weeklyList.Add(weekly);
            }
            return weeklyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.WeeklySheet;
            sheet.Headers.UpdateColumns();

            var dailySheet = DailyMapper.GetSheet();
            var keyRange = sheet.GetLocalRange(HeaderEnum.WEEK.GetDescription());
            var dailyKeyRange = dailySheet.GetRange(HeaderEnum.WEEK.GetDescription(), 2);

            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.WEEK:
                        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.WEEK.GetDescription(), dailySheet.GetRange(HeaderEnum.WEEK.GetDescription(), 2));
                        break;
                    case HeaderEnum.TRIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TRIPS.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.DAYS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, HeaderEnum.DAYS.GetDescription(), dailyKeyRange);
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.PAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.PAY.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.PAY.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIPS.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.TIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.BONUS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.BONUS.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TOTAL:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.CASH:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.CASH.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.CASH.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AMOUNT_PER_TRIP:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.DISTANCE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DISTANCE.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.DISTANCE;
                        break;
                    case HeaderEnum.AMOUNT_PER_DISTANCE:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.TIME_TOTAL.GetDescription(), dailyKeyRange, dailySheet.GetRange(HeaderEnum.TIME_TOTAL.GetDescription()));
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
                    case HeaderEnum.AVERAGE:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, HeaderEnum.AVERAGE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.NUMBER:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.NUMBER.GetDescription(), keyRange, "-", 1);
                        break;
                    case HeaderEnum.YEAR:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.YEAR.GetDescription(), keyRange, "-", 2);
                        break;
                    case HeaderEnum.DATE_BEGIN:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekBegin(keyRange, HeaderEnum.DATE_BEGIN.GetDescription(), sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription()), sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()));
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.DATE_END:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekEnd(keyRange, HeaderEnum.DATE_END.GetDescription(), sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription()), sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()));
                        header.Format = FormatEnum.DATE;
                        break;
                    default:
                        break;
                }
            });

            return sheet;
        }
    }
}
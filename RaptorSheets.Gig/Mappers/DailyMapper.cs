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
    public static class DailyMapper
    {
        public static List<DailyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var dailyList = new List<DailyEntity>();
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

                DailyEntity daily = new()
                {
                    RowId = id,
                    Date = HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                    Day = HeaderHelpers.GetStringValue(HeaderEnum.DAY.GetDescription(), value, headers),
                    Week = HeaderHelpers.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Month = HeaderHelpers.GetStringValue(HeaderEnum.MONTH.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    AmountPerTrip = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                    Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    Time = HeaderHelpers.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                    AmountPerTime = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                    AmountPerDistance = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers)
                };

                dailyList.Add(daily);
            }
            return dailyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.DailySheet;
            sheet.Headers.UpdateColumns();

            var shiftSheet = ShiftMapper.GetSheet();
            var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
            var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.DATE.GetDescription());

            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaUniqueFiltered(dateRange, HeaderEnum.DATE.GetDescription(), shiftSheet.GetRange(HeaderEnum.DATE.GetDescription(), 2));
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.WEEKDAY:
                        // This one is special - it uses day of week calculation
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.WEEKDAY.GetDescription()}\",ISBLANK({dateRange}), \"\", true,TEXT({dateRange}+1,\"ddd\")))";
                        break;
                    case HeaderEnum.DAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.DAY.GetDescription(), shiftSheet.GetRange(HeaderEnum.DAY.GetDescription(), 2));
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.WEEK:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekNumber(dateRange, HeaderEnum.WEEK.GetDescription(), dateRange);
                        break;
                    case HeaderEnum.MONTH:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaMonthNumber(dateRange, HeaderEnum.MONTH.GetDescription(), dateRange);
                        break;
                    case HeaderEnum.YEAR:
                        header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.YEAR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,YEAR({dateRange})))";
                        break;
                    case HeaderEnum.TRIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.TRIPS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TRIPS.GetDescription()));
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.DAYS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(dateRange, HeaderEnum.DAYS.GetDescription(), shiftKeyRange);
                        header.Format = FormatEnum.NUMBER;
                        break;
                    case HeaderEnum.PAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.PAY.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_PAY.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIPS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.TIPS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.BONUS:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.BONUS.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TOTAL:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaTotal(dateRange, HeaderEnum.TOTAL.GetDescription(), sheet.GetLocalRange(HeaderEnum.PAY.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIPS.GetDescription()), sheet.GetLocalRange(HeaderEnum.BONUS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.CASH:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.CASH.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_CASH.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.AMOUNT_PER_TRIP:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(dateRange, HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TRIPS.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.DISTANCE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.DISTANCE.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_DISTANCE.GetDescription()));
                        header.Format = FormatEnum.DISTANCE;
                        break;
                    case HeaderEnum.AMOUNT_PER_DISTANCE:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(dateRange, HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.DISTANCE.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    case HeaderEnum.TIME_TOTAL:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(dateRange, HeaderEnum.TIME_TOTAL.GetDescription(), shiftKeyRange, shiftSheet.GetRange(HeaderEnum.TOTAL_TIME.GetDescription()));
                        header.Format = FormatEnum.DURATION;
                        break;
                    case HeaderEnum.AMOUNT_PER_TIME:
                        header.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(dateRange, HeaderEnum.AMOUNT_PER_TIME.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()), sheet.GetLocalRange(HeaderEnum.TIME_TOTAL.GetDescription()));
                        header.Format = FormatEnum.ACCOUNTING;
                        break;
                    default:
                        break;
                }
            });

            return sheet;
        }
    }
}
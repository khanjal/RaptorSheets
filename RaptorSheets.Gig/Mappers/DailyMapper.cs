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

            // Configure common aggregation patterns from shift data
            MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, dateRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
            
            // Configure common ratio calculations
            MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, dateRange);

            // Configure specific headers unique to DailyMapper
            sheet.Headers.ForEach(header =>
            {
                var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(HeaderEnum.DATE.GetDescription(), shiftSheet.GetRange(HeaderEnum.DATE.GetDescription(), 2));
                        header.Format = FormatEnum.DATE;
                        break;
                    case HeaderEnum.WEEKDAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(dateRange, HeaderEnum.WEEKDAY.GetDescription(), dateRange);
                        break;
                    case HeaderEnum.DAY:
                        header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekday(dateRange, HeaderEnum.DAY.GetDescription(), dateRange);
                        header.Format = FormatEnum.NUMBER;
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
                    default:
                        break;
                }
            });

            return sheet;
        }
    }
}


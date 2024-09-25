using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Utilities;
using RaptorLoggerEngine.Utilities.Extensions;
using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;

namespace RaptorLoggerEngine.Mappers
{
    public static class DailyMapper
    {
        public static List<DailyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var dailyList = new List<DailyEntity>();
            var headers = new Dictionary<int, string>();
            var id = 0;

            foreach (var value in values)
            {
                id++;
                if (id == 1)
                {
                    headers = HeaderHelper.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                DailyEntity daily = new()
                {
                    Id = id,
                    Date = HeaderHelper.GetStringValue(HeaderEnum.DATE.DisplayName(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    AmountPerTrip = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.DisplayName(), value, headers),
                    Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    AmountPerDistance = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(), value, headers),
                    Time = HeaderHelper.GetStringValue(HeaderEnum.TIME_TOTAL.DisplayName(), value, headers),
                    AmountPerTime = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.DisplayName(), value, headers),
                    Day = HeaderHelper.GetStringValue(HeaderEnum.DAY.DisplayName(), value, headers),
                    Week = HeaderHelper.GetStringValue(HeaderEnum.WEEK.DisplayName(), value, headers),
                    Month = HeaderHelper.GetStringValue(HeaderEnum.MONTH.DisplayName(), value, headers),
                };

                dailyList.Add(daily);
            }
            return dailyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.DailySheet;

            var shiftSheet = ShiftMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.DATE);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DATE);

            // Day
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DAY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DAY.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},2)))",
                Format = FormatEnum.NUMBER
            });
            // Weekday
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.WEEKDAY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEKDAY.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},1)))",
                Format = FormatEnum.WEEKDAY
            });
            // Week
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.WEEK.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEK.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKNUM({sheetKeyRange},2)&\"-\"&YEAR({sheetKeyRange})))"
            });
            //  Month
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.MONTH.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.MONTH.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,MONTH({sheetKeyRange})&\"-\"&YEAR({sheetKeyRange})))"
            });

            return sheet;
        }
    }
}
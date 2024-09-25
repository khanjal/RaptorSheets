using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Utilities;
using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;

namespace RaptorLoggerEngine.Mappers
{
    public static class WeeklyMapper
    {
        public static List<WeeklyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var weeklyList = new List<WeeklyEntity>();
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

                WeeklyEntity weekly = new()
                {
                    Id = id,
                    Week = HeaderHelper.GetStringValue(HeaderEnum.WEEK.DisplayName(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Days = HeaderHelper.GetIntValue(HeaderEnum.DAYS.DisplayName(), value, headers),
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
                    Average = HeaderHelper.GetDecimalValue(HeaderEnum.AVERAGE.DisplayName(), value, headers),
                    AmountPerDay = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.DisplayName(), value, headers),
                    Number = HeaderHelper.GetIntValue(HeaderEnum.NUMBER.DisplayName(), value, headers),
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.DisplayName(), value, headers),
                    Begin = HeaderHelper.GetDateValue(HeaderEnum.DATE_BEGIN.DisplayName(), value, headers),
                    End = HeaderHelper.GetDateValue(HeaderEnum.DATE_END.DisplayName(), value, headers),
                };

                weeklyList.Add(weekly);
            }
            return weeklyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.WeeklySheet;

            var dailySheet = DailyMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.WEEK);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.WEEK);

            // #
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.NUMBER.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.NUMBER.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,IFERROR(INDEX(SPLIT({sheetKeyRange}, \"-\"), 0,1), 0)))"
            });

            // Year
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.YEAR.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.YEAR.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,IFERROR(INDEX(SPLIT({sheetKeyRange}, \"-\"), 0,2), 0)))"
            });

            // Begin
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE_BEGIN.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DATE_BEGIN.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,DATE({sheet.GetLocalRange(HeaderEnum.YEAR)},1,1)+(({sheet.GetLocalRange(HeaderEnum.NUMBER)}-1)*7)-WEEKDAY(DATE({sheet.GetLocalRange(HeaderEnum.YEAR)},1,1),3)))",
                Format = FormatEnum.DATE
            });

            // End
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE_END.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DATE_END.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\",true,DATE({sheet.GetLocalRange(HeaderEnum.YEAR)},1,7)+(({sheet.GetLocalRange(HeaderEnum.NUMBER)}-1)*7)-WEEKDAY(DATE({sheet.GetLocalRange(HeaderEnum.YEAR)},1,1),3)))",
                Format = FormatEnum.DATE
            });

            return sheet;
        }
    }
}
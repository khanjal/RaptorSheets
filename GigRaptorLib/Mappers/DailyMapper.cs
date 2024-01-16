using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
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
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                DailyEntity daily = new()
                {
                    Id = id,
                    Date = HeaderParser.GetStringValue(HeaderEnum.DATE.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    AmountPerTrip = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    AmountPerDistance = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(), value, headers),
                    Time = HeaderParser.GetStringValue(HeaderEnum.TIME_TOTAL.DisplayName(), value, headers),
                    AmountPerTime = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.DisplayName(), value, headers),
                    Day = HeaderParser.GetStringValue(HeaderEnum.DAY.DisplayName(), value, headers),
                    Week = HeaderParser.GetStringValue(HeaderEnum.WEEK.DisplayName(), value, headers),
                    Month = HeaderParser.GetStringValue(HeaderEnum.MONTH.DisplayName(), value, headers),
                };

                dailyList.Add(daily);
            }
            return dailyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = new SheetModel
            {
                Name = SheetEnum.DAILY.DisplayName(),
                TabColor = ColorEnum.LIGHT_GREEN,
                CellColor = ColorEnum.LIGHT_GRAY,
                FreezeColumnCount = 1,
                FreezeRowCount = 1,
                ProtectSheet = true
            };

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
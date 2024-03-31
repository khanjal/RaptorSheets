using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class WeekdayMapper
    {
        public static List<WeekdayEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var weekdays = new List<WeekdayEntity>();
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

                // Console.Write(JsonSerializer.Serialize(value));
                WeekdayEntity weekday = new()
                {
                    Id = id,
                    Day = HeaderParser.GetIntValue(HeaderEnum.DAY.DisplayName(), value, headers),
                    Weekday = HeaderParser.GetStringValue(HeaderEnum.WEEKDAY.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    Time = HeaderParser.GetStringValue(HeaderEnum.TIME_TOTAL.DisplayName(), value, headers),
                    Days = HeaderParser.GetIntValue(HeaderEnum.DAYS.DisplayName(), value, headers),
                    DailyAverage = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.DisplayName(), value, headers),
                    PreviousDailyAverage = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.DisplayName(), value, headers),
                    CurrentAmount = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_CURRENT.DisplayName(), value, headers),
                    PreviousAmount = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PREVIOUS.DisplayName(), value, headers),
                };

                weekdays.Add(weekday);
            }
            return weekdays;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.WeekdaySheet;

            var dailySheet = DailyMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.DAY);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DAY);

            // Curr Amt
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_CURRENT.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_CURRENT.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\", true,IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{sheetKeyRange},{SheetEnum.DAILY.DisplayName()}!{dailySheet.GetColumn(HeaderEnum.DATE)}:{dailySheet.GetColumn(HeaderEnum.TOTAL)},{dailySheet.GetIndex(HeaderEnum.TOTAL)}+1,false),0)))",
                Format = FormatEnum.ACCOUNTING
            });

            // Prev Amt
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PREVIOUS.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_PREVIOUS.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\", true,IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{sheetKeyRange}-7,{SheetEnum.DAILY.DisplayName()}!{dailySheet.GetColumn(HeaderEnum.DATE)}:{dailySheet.GetColumn(HeaderEnum.TOTAL)},{dailySheet.GetIndex(HeaderEnum.TOTAL)}+1,false),0)))",
                Format = FormatEnum.ACCOUNTING
            });

            // Prev Avg =ARRAYFORMULA(IFS(ROW(A1:A)=1,"Prev/Day",ISBLANK(A1:A), "", C1:C = 0, 0,true,(G1:G-P1:P)/IF(C1:C=0,1,C1:C-IF(P1:P=0,0,-1))))
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.DisplayName(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.DisplayName()}\",ISBLANK({sheetKeyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL)} = 0, 0,true,({sheet.GetLocalRange(HeaderEnum.TOTAL)}-{sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS)})/IF({sheet.GetLocalRange(HeaderEnum.DAYS)}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS)}-IF({sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS)}=0,0,-1))))",
                Format = FormatEnum.ACCOUNTING
            });

            return sheet;
        }
    }
}
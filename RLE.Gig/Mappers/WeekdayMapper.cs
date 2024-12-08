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
        var id = 0;

        foreach (var value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
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
                Day = HeaderHelpers.GetIntValue(HeaderEnum.DAY.GetDescription(), value, headers),
                Weekday = HeaderHelpers.GetStringValue(HeaderEnum.WEEKDAY.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
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

        var dailySheet = DailyMapper.GetSheet();

        sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.DAY);
        var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DAY.GetDescription());

        // Curr Amt
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_CURRENT.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_CURRENT.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\", true,IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{sheetKeyRange},{SheetEnum.DAILY.GetDescription()}!{dailySheet.GetColumn(HeaderEnum.DATE.GetDescription())}:{dailySheet.GetColumn(HeaderEnum.TOTAL.GetDescription())},{dailySheet.GetIndex(HeaderEnum.TOTAL.GetDescription())}+1,false),0)))",
            Format = FormatEnum.ACCOUNTING
        });

        // Prev Amt
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PREVIOUS.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_PREVIOUS.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\", true,IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{sheetKeyRange}-7,{SheetEnum.DAILY.GetDescription()}!{dailySheet.GetColumn(HeaderEnum.DATE.GetDescription())}:{dailySheet.GetColumn(HeaderEnum.TOTAL.GetDescription())},{dailySheet.GetIndex(HeaderEnum.TOTAL.GetDescription())}+1,false),0)))",
            Format = FormatEnum.ACCOUNTING
        });

        // Prev Avg =ARRAYFORMULA(IFS(ROW(A1:A)=1,"Prev/Day",ISBLANK(A1:A), "", C1:C = 0, 0,true,(G1:G-P1:P)/IF(C1:C=0,1,C1:C-IF(P1:P=0,0,-1))))
        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(),
            Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\", {sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())} = 0, 0,true,({sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription())}-{sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS.GetDescription())})/IF({sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())}=0,1,{sheet.GetLocalRange(HeaderEnum.DAYS.GetDescription())}-IF({sheet.GetLocalRange(HeaderEnum.AMOUNT_PREVIOUS.GetDescription())}=0,0,-1))))",
            Format = FormatEnum.ACCOUNTING
        });

        return sheet;
    }
}
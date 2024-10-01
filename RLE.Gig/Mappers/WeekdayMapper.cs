using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Helpers;

namespace RLE.Gig.Mappers;

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
                headers = HeaderHelper.ParserHeader(value);
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
                Day = HeaderHelper.GetIntValue(HeaderEnum.DAY.GetDescription(), value, headers),
                Weekday = HeaderHelper.GetStringValue(HeaderEnum.WEEKDAY.GetDescription(), value, headers),
                Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Time = HeaderHelper.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                Days = HeaderHelper.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
                DailyAverage = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                PreviousDailyAverage = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_PREVIOUS_DAY.GetDescription(), value, headers),
                CurrentAmount = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_CURRENT.GetDescription(), value, headers),
                PreviousAmount = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PREVIOUS.GetDescription(), value, headers),
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
using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Helpers;

namespace RLE.Gig.Mappers
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
                    Week = HeaderHelper.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Days = HeaderHelper.GetIntValue(HeaderEnum.DAYS.GetDescription(), value, headers),
                    Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    AmountPerTrip = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.GetDescription(), value, headers),
                    Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                    AmountPerDistance = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription(), value, headers),
                    Time = HeaderHelper.GetStringValue(HeaderEnum.TIME_TOTAL.GetDescription(), value, headers),
                    AmountPerTime = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.GetDescription(), value, headers),
                    Average = HeaderHelper.GetDecimalValue(HeaderEnum.AVERAGE.GetDescription(), value, headers),
                    AmountPerDay = HeaderHelper.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.GetDescription(), value, headers),
                    Number = HeaderHelper.GetIntValue(HeaderEnum.NUMBER.GetDescription(), value, headers),
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers),
                    Begin = HeaderHelper.GetDateValue(HeaderEnum.DATE_BEGIN.GetDescription(), value, headers),
                    End = HeaderHelper.GetDateValue(HeaderEnum.DATE_END.GetDescription(), value, headers),
                };

                weeklyList.Add(weekly);
            }
            return weeklyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.WeeklySheet;

            var dailySheet = DailyMapper.GetSheet();

            sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.WEEK);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.WEEK.GetDescription());

            // #
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.NUMBER.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.NUMBER.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,IFERROR(INDEX(SPLIT({sheetKeyRange}, \"-\"), 0,1), 0)))"
            });

            // Year
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.YEAR.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.YEAR.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,IFERROR(INDEX(SPLIT({sheetKeyRange}, \"-\"), 0,2), 0)))"
            });

            // Begin
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE_BEGIN.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DATE_BEGIN.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,DATE({sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription())},1,1)+(({sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())}-1)*7)-WEEKDAY(DATE({sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription())},1,1),3)))",
                Format = FormatEnum.DATE
            });

            // End
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DATE_END.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DATE_END.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,DATE({sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription())},1,7)+(({sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription())}-1)*7)-WEEKDAY(DATE({sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription())},1,1),3)))",
                Format = FormatEnum.DATE
            });

            return sheet;
        }
    }
}
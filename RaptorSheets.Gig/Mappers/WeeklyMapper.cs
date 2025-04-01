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
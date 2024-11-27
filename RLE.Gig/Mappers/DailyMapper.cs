using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Helpers;
using RLE.Core.Models.Google;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Helpers;

namespace RLE.Gig.Mappers
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
                    headers = HeaderHelpers.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                DailyEntity daily = new()
                {
                    Id = id,
                    Date = HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                    Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
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
                    Day = HeaderHelpers.GetStringValue(HeaderEnum.DAY.GetDescription(), value, headers),
                    Week = HeaderHelpers.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Month = HeaderHelpers.GetStringValue(HeaderEnum.MONTH.GetDescription(), value, headers),
                };

                dailyList.Add(daily);
            }
            return dailyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.DailySheet;

            var shiftSheet = ShiftMapper.GetSheet();

            sheet.Headers = GigSheetHelpers.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.DATE);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());

            // Day
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.DAY.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.DAY.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},2)))",
                Format = FormatEnum.NUMBER
            });
            // Weekday
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.WEEKDAY.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEKDAY.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKDAY({sheetKeyRange},1)))",
                Format = FormatEnum.WEEKDAY
            });
            // Week
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.WEEK.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.WEEK.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,WEEKNUM({sheetKeyRange},2)&\"-\"&YEAR({sheetKeyRange})))"
            });
            //  Month
            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = HeaderEnum.MONTH.GetDescription(),
                Formula = $"=ARRAYFORMULA(IFS(ROW({sheetKeyRange})=1,\"{HeaderEnum.MONTH.GetDescription()}\",ISBLANK({sheetKeyRange}), \"\",true,MONTH({sheetKeyRange})&\"-\"&YEAR({sheetKeyRange})))"
            });

            return sheet;
        }
    }
}
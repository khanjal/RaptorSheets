using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Utilities;

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
                    Date = HeaderHelper.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
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
                    Day = HeaderHelper.GetStringValue(HeaderEnum.DAY.GetDescription(), value, headers),
                    Week = HeaderHelper.GetStringValue(HeaderEnum.WEEK.GetDescription(), value, headers),
                    Month = HeaderHelper.GetStringValue(HeaderEnum.MONTH.GetDescription(), value, headers),
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
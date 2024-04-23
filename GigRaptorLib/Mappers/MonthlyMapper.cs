using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class MonthlyMapper
    {
        public static List<MonthlyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var monthlyList = new List<MonthlyEntity>();
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

                MonthlyEntity monthly = new()
                {
                    Id = id,
                    Month = HeaderHelper.GetStringValue(HeaderEnum.MONTH.DisplayName(), value, headers),
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
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.DisplayName(), value, headers)
                };

                monthlyList.Add(monthly);
            }
            return monthlyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.MonthlySheet;

            var dailySheet = DailyMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.MONTH);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.MONTH);

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

            return sheet;
        }
    }
}
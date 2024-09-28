using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Utilities;

namespace RLE.Gig.Mappers
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
                    Month = HeaderHelper.GetStringValue(HeaderEnum.MONTH.GetDescription(), value, headers),
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
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers)
                };

                monthlyList.Add(monthly);
            }
            return monthlyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.MonthlySheet;

            var dailySheet = DailyMapper.GetSheet();

            sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(dailySheet, HeaderEnum.MONTH);
            var sheetKeyRange = sheet.GetLocalRange(HeaderEnum.MONTH.GetDescription());

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

            return sheet;
        }
    }
}
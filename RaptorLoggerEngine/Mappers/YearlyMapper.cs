using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Models;
using RaptorLoggerEngine.Utilities;
using RaptorLoggerEngine.Utilities.Extensions;
using RLE.Core.Entities;

namespace RaptorLoggerEngine.Mappers
{
    public static class YearlyMapper
    {
        public static List<YearlyEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var yearlyList = new List<YearlyEntity>();
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

                YearlyEntity yearly = new()
                {
                    Id = id,
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.DisplayName(), value, headers),
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
                };

                yearlyList.Add(yearly);
            }
            return yearlyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.YearlySheet;

            var monthlySheet = MonthlyMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(monthlySheet, HeaderEnum.YEAR);

            return sheet;
        }
    }
}
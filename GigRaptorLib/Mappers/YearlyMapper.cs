using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
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
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                YearlyEntity yearly = new()
                {
                    Id = id,
                    Year = HeaderParser.GetIntValue(HeaderEnum.YEAR.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Days = HeaderParser.GetIntValue(HeaderEnum.DAYS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    AmountPerTrip = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TRIP.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                    AmountPerDistance = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName(), value, headers),
                    Time = HeaderParser.GetStringValue(HeaderEnum.TIME_TOTAL.DisplayName(), value, headers),
                    AmountPerTime = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_TIME.DisplayName(), value, headers),
                    Average = HeaderParser.GetDecimalValue(HeaderEnum.AVERAGE.DisplayName(), value, headers),
                    AmountPerDay = HeaderParser.GetDecimalValue(HeaderEnum.AMOUNT_PER_DAY.DisplayName(), value, headers),
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
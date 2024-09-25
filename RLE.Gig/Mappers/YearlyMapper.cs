using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Utilities;

namespace RLE.Gig.Mappers
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
                    Year = HeaderHelper.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers),
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
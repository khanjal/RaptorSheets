using RLE.Core.Extensions;
using RLE.Core.Helpers;
using RLE.Core.Models.Google;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Helpers;

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
                    headers = HeaderHelpers.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                YearlyEntity yearly = new()
                {
                    Id = id,
                    Year = HeaderHelpers.GetIntValue(HeaderEnum.YEAR.GetDescription(), value, headers),
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
                };

                yearlyList.Add(yearly);
            }
            return yearlyList;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.YearlySheet;

            var monthlySheet = MonthlyMapper.GetSheet();

            sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(monthlySheet, HeaderEnum.YEAR);

            return sheet;
        }
    }
}
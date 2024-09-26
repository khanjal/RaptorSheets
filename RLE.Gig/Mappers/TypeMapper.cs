using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Utilities;

namespace RLE.Gig.Mappers
{
    public static class TypeMapper
    {
        public static List<TypeEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var types = new List<TypeEntity>();
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

                TypeEntity type = new()
                {
                    Id = id,
                    Type = HeaderHelper.GetStringValue(HeaderEnum.TYPE.GetDescription(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                    Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                    Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                    Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                    Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                    Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                    Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                };

                types.Add(type);
            }
            return types;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.TypeSheet;

            var tripSheet = TripMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.TYPE);

            return sheet;
        }
    }
}
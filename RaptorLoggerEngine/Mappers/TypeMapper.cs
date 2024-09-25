using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Models;
using RaptorLoggerEngine.Utilities;
using RaptorLoggerEngine.Utilities.Extensions;
using RLE.Core.Entities;

namespace RaptorLoggerEngine.Mappers
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
                    Type = HeaderHelper.GetStringValue(HeaderEnum.TYPE.DisplayName(), value, headers),
                    Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
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
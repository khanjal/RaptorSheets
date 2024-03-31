using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
    public static class AddressMapper
    {
        public static List<AddressEntity> MapFromRangeData(IList<IList<object>> values)
        {
            var addresses = new List<AddressEntity>();
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

                AddressEntity address = new()
                {
                    Id = id,
                    Address = HeaderParser.GetStringValue(HeaderEnum.ADDRESS.DisplayName(), value, headers),
                    Visits = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                };

                addresses.Add(address);
            }
            return addresses;
        }

        public static SheetModel GetSheet()
        {
            var sheet = SheetsConfig.AddressSheet;

            var tripSheet = TripMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.ADDRESS_END);

            return sheet;
        }
    }
}
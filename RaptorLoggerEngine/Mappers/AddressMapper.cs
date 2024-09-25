using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Utilities;
using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;

namespace RaptorLoggerEngine.Mappers;

public static class AddressMapper
{
    public static List<AddressEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var addresses = new List<AddressEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        var id = 0;

        foreach (List<object> value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelper.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            };

            AddressEntity address = new()
            {
                Id = id,
                Address = HeaderHelper.GetStringValue(HeaderEnum.ADDRESS.DisplayName(), value, headers),
                Visits = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
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
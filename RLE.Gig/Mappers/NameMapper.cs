using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Entities;
using RLE.Gig.Utilities;

namespace RLE.Gig.Mappers;

public static class NameMapper
{
    public static List<NameEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var names = new List<NameEntity>();
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

            NameEntity name = new()
            {
                Id = id,
                Name = HeaderHelper.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Visits = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelper.GetIntValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
            };

            names.Add(name);
        }
        return names;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.NameSheet;

        var tripSheet = TripMapper.GetSheet();

        sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.NAME);

        return sheet;
    }
}
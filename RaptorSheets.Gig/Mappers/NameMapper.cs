using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

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
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            };

            NameEntity name = new()
            {
                RowId = id,
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Visits = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetIntValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Saved = true
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
using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Constants;
using RLE.Gig.Utilities;

namespace RLE.Gig.Mappers;

public static class RegionMapper
{
    public static List<RegionEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var regions = new List<RegionEntity>();
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

            RegionEntity region = new()
            {
                Id = id,
                Region = HeaderHelper.GetStringValue(HeaderEnum.REGION.GetDescription(), value, headers),
                Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIP.GetDescription(), value, headers),
                Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
            };

            regions.Add(region);
        }
        return regions;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.RegionSheet;

        var shiftSheet = ShiftMapper.GetSheet();

        sheet.Headers = SheetHelper.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.REGION);

        return sheet;
    }
}
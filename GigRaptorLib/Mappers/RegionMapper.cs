using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Mappers
{
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
                    headers = HeaderParser.ParserHeader(value);
                    continue;
                }

                if (value[0].ToString() == "")
                {
                    continue;
                }

                RegionEntity region = new()
                {
                    Id = id,
                    Region = HeaderParser.GetStringValue(HeaderEnum.REGION.DisplayName(), value, headers),
                    Trips = HeaderParser.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                    Pay = HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                    Tip = HeaderParser.GetDecimalValue(HeaderEnum.TIP.DisplayName(), value, headers),
                    Bonus = HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                    Total = HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                    Cash = HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                    Distance = HeaderParser.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
                };

                regions.Add(region);
            }
            return regions;
        }

        public static SheetModel GetSheet()
        {
            var sheet = new SheetModel
            {
                Name = SheetEnum.REGIONS.DisplayName(),
                TabColor = ColorEnum.CYAN,
                CellColor = ColorEnum.LIGHT_CYAN,
                FreezeColumnCount = 1,
                FreezeRowCount = 1,
                ProtectSheet = true
            };

            var shiftSheet = ShiftMapper.GetSheet();

            sheet.Headers = SheetHelper.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.REGION);

            return sheet;
        }
    }
}
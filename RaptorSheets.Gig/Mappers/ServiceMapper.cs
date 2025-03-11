using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class ServiceMapper
{
    public static List<ServiceEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var services = new List<ServiceEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
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

            ServiceEntity service = new()
            {
                RowId = id,
                Service = HeaderHelpers.GetStringValue(HeaderEnum.SERVICE.GetDescription(), value, headers),
                Trips = HeaderHelpers.GetIntValue(HeaderEnum.TRIPS.GetDescription(), value, headers),
                Pay = HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), value, headers),
                Tip = HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), value, headers),
                Bonus = HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), value, headers),
                Total = HeaderHelpers.GetDecimalValue(HeaderEnum.TOTAL.GetDescription(), value, headers),
                Cash = HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), value, headers),
                Distance = HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), value, headers),
                Saved = true
            };

            services.Add(service);
        }
        return services;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ServiceSheet;

        var shiftSheet = ShiftMapper.GetSheet();

        sheet.Headers = GigSheetHelpers.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.SERVICE);

        return sheet;
    }
}
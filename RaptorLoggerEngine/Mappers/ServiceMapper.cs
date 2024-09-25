using RaptorLoggerEngine.Constants;
using RaptorLoggerEngine.Entities;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Models;
using RaptorLoggerEngine.Utilities;
using RaptorLoggerEngine.Utilities.Extensions;

namespace RaptorLoggerEngine.Mappers;

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
                headers = HeaderHelper.ParserHeader(value);
                continue;
            }

            if (value[0].ToString() == "")
            {
                continue;
            }

            ServiceEntity service = new()
            {
                Id = id,
                Service = HeaderHelper.GetStringValue(HeaderEnum.SERVICE.DisplayName(), value, headers),
                Trips = HeaderHelper.GetIntValue(HeaderEnum.TRIPS.DisplayName(), value, headers),
                Pay = HeaderHelper.GetDecimalValue(HeaderEnum.PAY.DisplayName(), value, headers),
                Tip = HeaderHelper.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), value, headers),
                Bonus = HeaderHelper.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), value, headers),
                Total = HeaderHelper.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), value, headers),
                Cash = HeaderHelper.GetDecimalValue(HeaderEnum.CASH.DisplayName(), value, headers),
                Distance = HeaderHelper.GetDecimalValue(HeaderEnum.DISTANCE.DisplayName(), value, headers),
            };

            services.Add(service);
        }
        return services;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ServiceSheet;

        var shiftSheet = ShiftMapper.GetSheet();

        sheet.Headers = SheetHelper.GetCommonShiftGroupSheetHeaders(shiftSheet, HeaderEnum.SERVICE);

        return sheet;
    }
}
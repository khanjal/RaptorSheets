using RaptorSheets.Common.Constants.SheetConfigs;
using RaptorSheets.Common.Enums;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Common.Mappers;

public class SetupMapper
{
    public static List<SetupEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var setupList = new List<SetupEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (var value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            SetupEntity setup = new()
            {
                RowId = id,
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Value = HeaderHelpers.GetStringValue(HeaderEnum.VALUE.GetDescription(), value, headers)
            };

            setupList.Add(setup);
        }
        return setupList;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SetupSheetConfig.SetupSheet;

        //var monthlySheet = MonthlyMapper.GetSheet();

        //sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(monthlySheet, HeaderEnum.NAME);

        return sheet;
    }
}

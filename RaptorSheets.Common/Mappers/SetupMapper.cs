using Google.Apis.Sheets.v4.Data;
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
                Value = HeaderHelpers.GetStringValue(HeaderEnum.VALUE.GetDescription(), value, headers),
                Saved = true
            };

            setupList.Add(setup);
        }
        return setupList;
    }

    public static IList<IList<object?>> MapToRangeData(List<SetupEntity> setup, IList<object> setupHeaders)
    {
        var rangeData = new List<IList<object?>>();

        foreach (var item in setup)
        {
            var objectList = new List<object?>();

            foreach (var header in setupHeaders)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                
                switch (headerEnum)
                {
                    case HeaderEnum.NAME:
                        objectList.Add(item.Name);
                        break;
                    case HeaderEnum.VALUE:
                        objectList.Add(item.Value);
                        break;
                    default:
                        objectList.Add(null);
                        break;
                }
            }

            rangeData.Add(objectList);
        }

        return rangeData;
    }

    public static IList<RowData> MapToRowData(List<SetupEntity> setupEntities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (SetupEntity item in setupEntities)
        {
            var rowData = new RowData();
            var cells = new List<CellData>();
            foreach (var header in headers)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                switch (headerEnum)
                {
                    case HeaderEnum.NAME:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = item.Name ?? null } });
                        break;
                    case HeaderEnum.VALUE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = item.Value ?? null } });
                        break;
                    default:
                        cells.Add(new CellData());
                        break;
                }
            }
            rowData.Values = cells;
            rows.Add(rowData);
        }

        return rows;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SetupSheetConfig.SetupSheet;

        return sheet;
    }
}

using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Tests.Data;

public class TestSheetData
{
    public static SheetModel GetModelData()
    {
        var sheet = new SheetModel
        {
            Name = "Test Sheet",
            Headers = []
        };

        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.FIRST_COLUMN.GetDescription(),
            Formula = "Formula",
            Format = FormatEnum.TEXT
        });

        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.SECOND_COLUMN.GetDescription(),
            Formula = "None",
            Format = FormatEnum.NUMBER
        });

        return sheet;
    }
}

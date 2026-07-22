using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Tests.Data;

public static class TestSheetData
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
            Name = Header.FIRST_COLUMN.GetDescription(),
            Formula = "Formula",
            Format = Format.TEXT
        });

        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = Header.SECOND_COLUMN.GetDescription(),
            Formula = "None",
            Format = Format.NUMBER
        });

        return sheet;
    }
}

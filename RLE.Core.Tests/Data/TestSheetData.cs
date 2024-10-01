using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Models.Google;

namespace RLE.Core.Tests.Data;

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

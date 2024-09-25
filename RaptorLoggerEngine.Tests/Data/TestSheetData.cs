using RLE.Core.Enums;
using RLE.Core.Models.Google;
using RLE.Core.Utilities.Extensions;

namespace RaptorLoggerEngine.Tests.Data;

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
            Name = HeaderEnum.WEEK.DisplayName(),
            Formula = "Formula",
            Format = FormatEnum.TEXT
        });

        sheet.Headers.AddColumn(new SheetCellModel
        {
            Name = HeaderEnum.DATE.DisplayName(),
            Formula = "None",
            Format = FormatEnum.NUMBER
        });

        return sheet;
    }
}

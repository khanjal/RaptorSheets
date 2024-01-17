using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Tests.Data
{
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
}

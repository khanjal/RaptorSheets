using RaptorSheets.Common.Enums;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Common.Constants.SheetConfigs
{
    public class SetupSheetConfig
    {
        public static SheetModel SetupSheet => new()
        {
            Name = SheetEnum.SETUP.GetDescription(),
            CellColor = ColorEnum.LIGHT_PURPLE,
            TabColor = ColorEnum.PURPLE,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = false,
            Headers = [
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.VALUE.GetDescription() }
        ]
        };
    }
}

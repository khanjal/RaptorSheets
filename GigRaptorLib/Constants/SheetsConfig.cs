using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Constants
{
    public static class SheetsConfig
    {
        public static SheetModel AddressSheet => new()
        {
            Name = SheetEnum.ADDRESSES.DisplayName(),
            CellColor = ColorEnum.LIGHT_CYAN,
            TabColor = ColorEnum.CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = [
                new SheetCellModel { Name = HeaderEnum.ADDRESS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TRIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.PAY.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TIPS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.BONUS.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.TOTAL.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.CASH.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_TRIP.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.AMOUNT_PER_DISTANCE.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.VISIT_FIRST.DisplayName() },
                new SheetCellModel { Name = HeaderEnum.VISIT_LAST.DisplayName() }
            ]
        };
    }
}

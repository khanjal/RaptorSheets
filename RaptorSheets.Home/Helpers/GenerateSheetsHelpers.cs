using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Enums;
using RaptorSheets.Home.Sheets;

namespace RaptorSheets.Home.Helpers;

public static class GenerateSheetsHelpers
{
    internal static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        return SheetGenerationHelper.Generate(sheets, GetSheetModel, GetDataValidation);
    }

    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    // Case-insensitive name -> factory lookup, same convention as SheetRegistry<TEntity>'s own
    // _factories dictionary - O(1) instead of a sequential switch, and reads as a flat table instead
    // of N near-identical "var s when string.Equals(...)" arms.
    private static readonly Dictionary<string, Func<SheetModel>> _sheetModelFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [SheetsConfig.SheetNames.Appliances] = ApplianceSheet.GetSheet,
        [SheetsConfig.SheetNames.Projects] = ProjectSheet.GetSheet,
        [SheetsConfig.SheetNames.Maintenance] = MaintenanceSheet.GetSheet,
        [SheetsConfig.SheetNames.DoorsWindows] = DoorWindowSheet.GetSheet,
        [SheetsConfig.SheetNames.Paints] = PaintSheet.GetSheet,
        [SheetsConfig.SheetNames.Power] = PowerSheet.GetSheet,
        [SheetsConfig.SheetNames.Rooms] = RoomSheet.GetSheet,
        [SheetsConfig.SheetNames.Contacts] = ContactSheet.GetSheet,
        [SheetsConfig.SheetNames.Stats] = StatSheet.GetSheet,
        // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
        // specific ad-hoc name.
        [SheetManagerBase.TempSheetName] = () => new SheetModel { Name = SheetManagerBase.TempSheetName },
    };

    private static SheetModel GetSheetModel(string sheet)
    {
        if (_sheetModelFactories.TryGetValue(sheet, out var factory))
        {
            return factory();
        }

        // Anything unrecognized is a genuine caller error and should still throw.
        throw new NotImplementedException($"Sheet model not found for: {sheet}");
    }

    private static DataValidationRule? GetDataValidation(SheetCellModel header)
    {
        var columnRange = $"{header.Column}2:{header.Column}";
        return HomeSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>(), columnRange);
    }
}

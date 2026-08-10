using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Sheets;

namespace RaptorSheets.Gig.Helpers;

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
        [SheetsConfig.SheetNames.Addresses] = AddressSheet.GetSheet,
        [SheetsConfig.SheetNames.Daily] = DailySheet.GetSheet,
        [SheetsConfig.SheetNames.Expenses] = ExpenseSheet.GetSheet,
        [SheetsConfig.SheetNames.Monthly] = MonthlySheet.GetSheet,
        [SheetsConfig.SheetNames.Names] = NameSheet.GetSheet,
        [SheetsConfig.SheetNames.Places] = PlaceSheet.GetSheet,
        [SheetsConfig.SheetNames.Deliveries] = DeliverySheet.GetSheet,
        [SheetsConfig.SheetNames.Locations] = LocationSheet.GetSheet,
        [SheetsConfig.SheetNames.Regions] = RegionSheet.GetSheet,
        [SheetsConfig.SheetNames.Services] = ServiceSheet.GetSheet,
        [SheetsConfig.SheetNames.Setup] = SetupSheet.GetSheet,
        [SheetsConfig.SheetNames.Shifts] = ShiftSheet.GetSheet,
        [SheetsConfig.SheetNames.Trips] = TripSheet.GetSheet,
        [SheetsConfig.SheetNames.Types] = TypeSheet.GetSheet,
        [SheetsConfig.SheetNames.Weekdays] = WeekdaySheet.GetSheet,
        [SheetsConfig.SheetNames.Weekly] = WeeklySheet.GetSheet,
        [SheetsConfig.SheetNames.Yearly] = YearlySheet.GetSheet,
        // DeleteSheets' temp-sheet safety mechanism (SheetManagerBase<TEntity>.DeleteSheets) asks for
        // a bare AddSheet request for this specific ad-hoc, non-domain name.
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
        return GigSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>(), columnRange);
    }
}

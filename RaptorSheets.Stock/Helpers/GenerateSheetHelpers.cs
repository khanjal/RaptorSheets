using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Sheets;

namespace RaptorSheets.Stock.Helpers;

public static class GenerateSheetHelpers
{
    internal static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        return SheetGenerationHelper.Generate(sheets, GetSheetModel, GetDataValidation);
    }

    // Case-insensitive name -> factory lookup, same convention as SheetRegistry<TEntity>'s own
    // _factories dictionary (and now Gig/Home/Job's GenerateSheetsHelpers) - O(1) instead of a
    // string->enum round-trip plus a second switch on the enum.
    private static readonly Dictionary<string, Func<SheetModel>> _sheetModelFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [SheetName.ACCOUNTS.GetDescription()] = AccountSheet.GetSheet,
        [SheetName.STOCKS.GetDescription()] = StockSheet.GetSheet,
        [SheetName.TICKERS.GetDescription()] = TickerSheet.GetSheet,
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
        return StockSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>());
    }
}

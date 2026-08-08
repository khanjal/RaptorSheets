using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

public static class CoreTestSheetNames
{
    public const string Items = "Items";
    public const string Log = "Log";
    public const string Summary = "Summary";
}

public static class ItemSheetDefinition
{
    internal static SheetModel BaseSheet => new()
    {
        Name = CoreTestSheetNames.Items,
        TabColor = SheetColor.BLUE,
        CellColor = SheetColor.LIGHT_BLUE_3,
        // BLUE is in SheetColor's own "Dark" list (see its doc comment) - needs an explicit light
        // FontColor or the default BLACK header text is illegible against it.
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ItemEntity>()
    };

    public static SheetModel GetSheet() => BaseSheet;
}

public static class LogSheetDefinition
{
    internal static SheetModel BaseSheet => new()
    {
        Name = CoreTestSheetNames.Log,
        TabColor = SheetColor.GREEN,
        CellColor = SheetColor.LIGHT_GREEN_3,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<LogEntity>()
    };

    public static SheetModel GetSheet() => BaseSheet;
}

/// <summary>
/// The "calculated/child sheet" - fully formula-driven, cross-references Items (never StockSheet.cs-
/// style bare/formula-laden split needed here since nothing else depends on Summary in turn). Kept on
/// its own bare BaseSheet/GetSheet() split anyway to match the established domain convention.
/// </summary>
public static class SummarySheetDefinition
{
    internal static SheetModel BaseSheet => new()
    {
        Name = CoreTestSheetNames.Summary,
        TabColor = SheetColor.PURPLE,
        CellColor = SheetColor.LIGHT_PURPLE_3,
        // PURPLE is in SheetColor's "Dark" list too - same reasoning as Items' BLUE above.
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<SummaryEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var itemsSheet = ItemSheetDefinition.BaseSheet;
        itemsSheet.Headers.UpdateColumns();

        // keyRange always means "this sheet's own column A" - here that's Category, which is a real
        // populated key column (SORT/UNIQUE below), not an empty self-reference. Total/Count's
        // ARRAYFORMULAs spread down as far as Category has rows, exactly like Stock's AccountSheet
        // keys Shares/AverageCost off its own Account column.
        var keyRange = GoogleConfig.KeyRange;

        var categoryHeader = sheet.Headers.First(h => h.Name == "Category");
        categoryHeader.Formula = ColumnFormulas.SortUnique("Category", itemsSheet.GetRange("Category", 2));

        var totalHeader = sheet.Headers.First(h => h.Name == "Total");
        totalHeader.Formula = ColumnFormulas.SumIf(
            "Total",
            keyRange,
            itemsSheet.GetRange("Category"),
            keyRange,
            itemsSheet.GetRange("Amount"));

        var countHeader = sheet.Headers.First(h => h.Name == "Count");
        countHeader.Formula = ColumnFormulas.CountIf(
            "Count",
            keyRange,
            itemsSheet.GetRange("Category"),
            keyRange);

        return sheet;
    }
}

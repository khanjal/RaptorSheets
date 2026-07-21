using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Registries;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;
using RaptorSheets.Stock.Entities;

namespace RaptorSheets.Stock.Helpers;

public static class StockSheetHelpers
{
    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            AccountMapper.GetSheet(),
            StockMapper.GetSheet(),
            TickerMapper.GetSheet()
        };

        return sheets;
    }

    /// <summary>
    /// The shared registry backing this domain's header/row-mapping/missing-column orchestration.
    /// Exposed so <see cref="RaptorSheets.Core.Managers.GoogleSheetManagerBase"/>'s generic
    /// GetSheetsCoreAsync/AutoHealMissingColumnsAsync can operate on it directly.
    /// </summary>
    public static SheetRegistry<SheetEntity> Registry => s_registry;

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        registry.Register(SheetEnum.ACCOUNTS.GetDescription(), () => SheetsConfig.AccountSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.AccountSheet));
            se.Sheets.Accounts = AccountMapper.MapFromRangeData(values);
        });

        registry.Register(SheetEnum.STOCKS.GetDescription(), () => SheetsConfig.StockSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.StockSheet));
            se.Sheets.Stocks = StockMapper.MapFromRangeData(values);
        });

        registry.Register(SheetEnum.TICKERS.GetDescription(), () => SheetsConfig.TickerSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.TickerSheet));
            se.Sheets.Tickers = TickerMapper.MapFromRangeData(values);
        });

        return registry;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var canonicalNames = Enum.GetValues<SheetEnum>().Select(e => e.GetDescription());
        return s_registry.GetMissingSheets(spreadsheet, canonicalNames);
    }

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Stock sheet.
    /// Only needs sheet tab metadata (no grid/cell data).
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        return s_registry.CheckUnknownSheets(spreadsheet);
    }

    /// <summary>
    /// Full header validation against grid-data (IncludeGridData=true) spreadsheet metadata.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return s_registry.CheckSheetHeaders(spreadsheet);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with
    /// <see cref="RaptorSheets.Core.Helpers.ColumnInsertionHelper"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return s_registry.CheckSheetHeaders(spreadsheet, out missingColumns);
    }

    /// <summary>
    /// Detects columns missing entirely from a batchGet response, reusing the header row already
    /// present in each range - no extra API call. SheetId is left at 0; the caller fills it in.
    /// </summary>
    public static Dictionary<string, List<ColumnInsertionInfo>> DetectMissingColumns(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.DetectMissingColumns(response);
    }

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return s_registry.GetSheetLayout(sheetName);
    }

    public static List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        return s_registry.GetSheetLayouts(sheetNames);
    }

    public static DataValidationRule GetDataValidation(ValidationEnum validation)
    {
        var dataValidation = new DataValidationRule();

        switch (validation)
        {
            case ValidationEnum.BOOLEAN:
                dataValidation.Condition = new BooleanCondition { Type = "BOOLEAN" };
                break;
            case ValidationEnum.RANGE_ACCOUNT:
            case ValidationEnum.RANGE_TICKER:
                var values = new List<ConditionValue> { new() { UserEnteredValue = $"={GetSheetForRange(validation)?.GetDescription()}!A2:A" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = values };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
        }

        return dataValidation;
    }

    private static SheetEnum? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ACCOUNT => SheetEnum.ACCOUNTS,
            ValidationEnum.RANGE_TICKER => SheetEnum.TICKERS,
            _ => null
        };
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.MapData(response);
    }
}

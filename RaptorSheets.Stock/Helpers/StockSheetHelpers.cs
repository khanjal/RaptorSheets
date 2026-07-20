using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Extensions;
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

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        registry.Register(SheetEnum.ACCOUNTS.GetDescription(), () => SheetsConfig.AccountSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.AccountSheet));
            se.Accounts = AccountMapper.MapFromRangeData(values);
        });

        registry.Register(SheetEnum.STOCKS.GetDescription(), () => SheetsConfig.StockSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.StockSheet));
            se.Stocks = StockMapper.MapFromRangeData(values);
        });

        registry.Register(SheetEnum.TICKERS.GetDescription(), () => SheetsConfig.TickerSheet, (se, values) =>
        {
            var headers = values[0];
            se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.TickerSheet));
            se.Tickers = TickerMapper.MapFromRangeData(values);
        });

        return registry;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var canonicalNames = Enum.GetValues<SheetEnum>().Select(e => e.GetDescription());
        return s_registry.GetMissingSheets(spreadsheet, canonicalNames);
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

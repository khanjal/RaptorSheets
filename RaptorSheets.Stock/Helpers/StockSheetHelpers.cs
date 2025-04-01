using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Constants;

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

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheetData = new List<SheetModel>();

        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames<SheetEnum>())
        {
            SheetEnum sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), name);

            if (spreadsheetSheets.Contains(name))
            {
                continue;
            }

            // Get data for each missing sheet.
            switch (sheetEnum)
            {
                case SheetEnum.ACCOUNTS:
                    sheetData.Add(AccountMapper.GetSheet());
                    break;
                case SheetEnum.STOCKS:
                    sheetData.Add(StockMapper.GetSheet());
                    break;
                case SheetEnum.TICKERS:
                    sheetData.Add(TickerMapper.GetSheet());
                    break;
                default:
                    break;
            }
        }

        return sheetData;
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
        if (response.ValueRanges == null)
        {
            return null;
        }

        var sheet = new SheetEntity();

        // TODO: Figure out a better way to handle looping with message and entity mapping in the switch.
        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetRange = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;
            var headers = values[0];

            Enum.TryParse(sheetRange.ToUpper(), out SheetEnum sheetEnum);

            switch (sheetEnum)
            {
                case SheetEnum.ACCOUNTS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.AccountSheet));
                    sheet.Accounts = AccountMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.STOCKS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.StockSheet));
                    sheet.Stocks = StockMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.TICKERS:
                    sheet.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, SheetsConfig.TickerSheet));
                    sheet.Tickers = TickerMapper.MapFromRangeData(values);
                    break;
            }
        }

        return sheet;
    }
}
using Google.Apis.Sheets.v4.Data;
using RLE.Core.Constants;
using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Models.Google;
using RLE.Core.Helpers;
using RLE.Stock.Enums;
using RLE.Stock.Mappers;
using RLE.Stock.Entities;
using RLE.Stock.Constants;

namespace RLE.Stock.Helpers;

public static class StockSheetHelpers
{
    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            //AccountMapper.GetSheet(),
            //TripMapper.GetSheet()
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
                    // sheetData.Add(AddressMapper.GetSheet());
                    break;
                case SheetEnum.STOCKS:
                    sheetData.Add(StockMapper.GetSheet());
                    break;
                case SheetEnum.TICKERS:
                    //sheetData.Add(TickerMapper.GetSheet());
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
            case ValidationEnum.RANGE_ADDRESS:
            case ValidationEnum.RANGE_NAME:
            case ValidationEnum.RANGE_PLACE:
            case ValidationEnum.RANGE_REGION:
            case ValidationEnum.RANGE_SERVICE:
            case ValidationEnum.RANGE_TYPE:
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
            //ValidationEnum.RANGE_ADDRESS => SheetEnum.ADDRESSES,
            //ValidationEnum.RANGE_NAME => SheetEnum.NAMES,
            //ValidationEnum.RANGE_PLACE => SheetEnum.PLACES,
            //ValidationEnum.RANGE_REGION => SheetEnum.REGIONS,
            //ValidationEnum.RANGE_SERVICE => SheetEnum.SERVICES,
            //ValidationEnum.RANGE_TYPE => SheetEnum.TYPES,
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

            Enum.TryParse(sheetRange.ToUpper(), out SheetEnum sheetEnum);

            switch (sheetEnum)
            {
                case SheetEnum.ACCOUNTS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, SheetsConfig.AccountSheet));
                    sheet.Accounts = AccountMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.STOCKS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, SheetsConfig.StockSheet));
                    sheet.Stocks = StockMapper.MapFromRangeData(values);
                    break;
                case SheetEnum.TICKERS:
                    sheet.Messages.AddRange(HeaderHelper.CheckSheetHeaders(values, SheetsConfig.TickerSheet));
                    sheet.Tickers = TickerMapper.MapFromRangeData(values);
                    break;
            }
        }

        return sheet;
    }
}
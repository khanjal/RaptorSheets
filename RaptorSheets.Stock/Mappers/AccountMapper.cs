using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using Header = RaptorSheets.Stock.Enums.Header;

namespace RaptorSheets.Stock.Mappers;

public static class AccountMapper
{
    public static List<AccountEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var entities = new List<AccountEntity>();
        var headers = new Dictionary<int, string>();
        var filteredValues = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).Select(x => x.ToList()).ToList();
        var id = 0;

        foreach (var value in filteredValues)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            }

            AccountEntity entity = new()
            {
                RowId = id,
                Account = HeaderHelpers.GetStringValue(Header.ACCOUNT.GetDescription(), value, headers),
                Stocks = HeaderHelpers.GetDecimalValue(Header.STOCKS.GetDescription(), value, headers),
                Shares = HeaderHelpers.GetDecimalValue(Header.SHARES.GetDescription(), value, headers),
                AverageCost = HeaderHelpers.GetDecimalValue(Header.AVERAGE_COST.GetDescription(), value, headers),
                CostTotal = HeaderHelpers.GetDecimalValue(Header.COST_TOTAL.GetDescription(), value, headers),
                CurrentPrice = HeaderHelpers.GetDecimalValue(Header.CURRENT_PRICE.GetDescription(), value, headers),
                CurrentTotal = HeaderHelpers.GetDecimalValue(Header.CURRENT_TOTAL.GetDescription(), value, headers),
                Return = HeaderHelpers.GetDecimalValue(Header.RETURN.GetDescription(), value, headers),
            };

            entities.Add(entity);
        }
        return entities;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.AccountSheet;
        sheet.Headers.UpdateColumns();

        var stockSheet = SheetsConfig.StockSheet;
        stockSheet.Headers.UpdateColumns();

        var keyRange = GoogleConfig.KeyRange;
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.ACCOUNT:
                    header.Formula = ColumnFormulas.SortUnique(headerEnum.GetDescription(),
                                                                stockSheet.GetRange(Header.ACCOUNT.GetDescription(), 2));
                    break;

                case Header.AVERAGE_COST:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIfDivide(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(Header.ACCOUNT.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()),
                                                                    sheet.GetRange(Header.STOCKS.GetDescription()));
                    break;

                case Header.COST_TOTAL:
                case Header.CURRENT_TOTAL:
                case Header.SHARES:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(Header.ACCOUNT.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()));
                    break;

                case Header.RETURN:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(Header.COST_TOTAL.GetDescription()));
                    break;

                case Header.STOCKS:
                    header.Formula = ColumnFormulas.CountIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(Header.ACCOUNT.GetDescription()),
                                                                    keyRange);
                    break;

                default:
                    break;
            }
        });

        return sheet;
    }
}
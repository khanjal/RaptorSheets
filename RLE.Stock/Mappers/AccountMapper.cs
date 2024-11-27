using RLE.Core.Constants;
using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Helpers;
using RLE.Core.Models.Google;
using RLE.Stock.Constants;
using RLE.Stock.Entities;
using RLE.Stock.Enums;

namespace RLE.Stock.Mappers;

public static class AccountMapper
{
    public static List<AccountEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var entities = new List<AccountEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        var id = 0;

        foreach (List<object> value in values)
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
            };

            AccountEntity entity = new()
            {
                Id = id,
                Account = HeaderHelpers.GetStringValue(HeaderEnum.ACCOUNT.GetDescription(), value, headers),
                Stocks = HeaderHelpers.GetDecimalValue(HeaderEnum.STOCKS.GetDescription(), value, headers),
                Shares = HeaderHelpers.GetDecimalValue(HeaderEnum.SHARES.GetDescription(), value, headers),
                AverageCost = HeaderHelpers.GetDecimalValue(HeaderEnum.AVERAGE_COST.GetDescription(), value, headers),
                CostTotal = HeaderHelpers.GetDecimalValue(HeaderEnum.COST_TOTAL.GetDescription(), value, headers),
                CurrentPrice = HeaderHelpers.GetDecimalValue(HeaderEnum.CURRENT_PRICE.GetDescription(), value, headers),
                CurrentTotal = HeaderHelpers.GetDecimalValue(HeaderEnum.CURRENT_TOTAL.GetDescription(), value, headers),
                Return = HeaderHelpers.GetDecimalValue(HeaderEnum.RETURN.GetDescription(), value, headers),
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
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.ACCOUNT:
                    header.Formula = ColumnFormulas.SortUnique(headerEnum.GetDescription(),
                                                                stockSheet.GetRange(HeaderEnum.ACCOUNT.GetDescription(), 2));
                    break;

                case HeaderEnum.AVERAGE_COST:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIfDivide(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(HeaderEnum.ACCOUNT.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()),
                                                                    sheet.GetRange(HeaderEnum.STOCKS.GetDescription()));
                    break;

                case HeaderEnum.COST_TOTAL:
                case HeaderEnum.CURRENT_TOTAL:
                case HeaderEnum.SHARES:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(HeaderEnum.ACCOUNT.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()));
                    break;

                case HeaderEnum.RETURN:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(HeaderEnum.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(HeaderEnum.COST_TOTAL.GetDescription()));
                    break;

                case HeaderEnum.STOCKS:
                    header.Formula = ColumnFormulas.CountIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(HeaderEnum.ACCOUNT.GetDescription()),
                                                                    keyRange);
                    break;

                default:
                    break;
            }
        });

        return sheet;
    }
}
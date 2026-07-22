using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using Header = RaptorSheets.Stock.Enums.Header;

namespace RaptorSheets.Stock.Mappers;

public static class TickerMapper
{
    public static List<TickerEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var entities = new List<TickerEntity>();
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

            TickerEntity entity = new()
            {
                RowId = id,
                Ticker = HeaderHelpers.GetStringValue(Header.TICKER.GetDescription(), value, headers),
                Name = HeaderHelpers.GetStringValue(Header.NAME.GetDescription(), value, headers),
                Accounts = HeaderHelpers.GetIntValue(Header.ACCOUNTS.GetDescription(), value, headers),
                Shares = HeaderHelpers.GetDecimalValue(Header.SHARES.GetDescription(), value, headers),
                AverageCost = HeaderHelpers.GetDecimalValue(Header.AVERAGE_COST.GetDescription(), value, headers),
                CostTotal = HeaderHelpers.GetDecimalValue(Header.COST_TOTAL.GetDescription(), value, headers),
                CurrentPrice = HeaderHelpers.GetDecimalValue(Header.CURRENT_PRICE.GetDescription(), value, headers),
                CurrentTotal = HeaderHelpers.GetDecimalValue(Header.CURRENT_TOTAL.GetDescription(), value, headers),
                Return = HeaderHelpers.GetDecimalValue(Header.RETURN.GetDescription(), value, headers),
                PeRatio = HeaderHelpers.GetDecimalValue(Header.PE_RATIO.GetDescription(), value, headers),
                WeekHigh52 = HeaderHelpers.GetDecimalValue(Header.WEEK_HIGH_52.GetDescription(), value, headers),
                WeekLow52 = HeaderHelpers.GetDecimalValue(Header.WEEK_LOW_52.GetDescription(), value, headers),
                MaxHigh = HeaderHelpers.GetDecimalValue(Header.MAX_HIGH.GetDescription(), value, headers),
                MinLow = HeaderHelpers.GetDecimalValue(Header.MIN_LOW.GetDescription(), value, headers)
            };

            entities.Add(entity);
        }
        return entities;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.TickerSheet;
        sheet.Headers.UpdateColumns();

        var stockSheet = SheetsConfig.StockSheet;
        stockSheet.Headers.UpdateColumns();

        var keyRange = GoogleConfig.KeyRange;
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.ACCOUNTS:
                    header.Formula = ColumnFormulas.CountIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(Header.TICKER.GetDescription()),
                                                                    keyRange);
                    break;

                case Header.AVERAGE_COST:
                    header.Note = ColumnNotes.AverageCost;
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.DivideRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.COST_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(Header.SHARES.GetDescription()));
                    break;

                case Header.COST_TOTAL:
                case Header.CURRENT_TOTAL:
                case Header.SHARES:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(Header.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()));
                    break;

                case Header.CURRENT_PRICE:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.PRICE.GetDescription());
                    break;

                case Header.MAX_HIGH:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceMax(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.HIGH.GetDescription());
                    break;

                case Header.MIN_LOW:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceMin(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.LOW.GetDescription());
                    break;

                case Header.NAME:
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.NAME.GetDescription());
                    break;

                case Header.PE_RATIO:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.PE_RATIO.GetDescription());
                    break;

                case Header.RETURN:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(Header.COST_TOTAL.GetDescription()));
                    break;

                case Header.TICKER:
                    header.Formula = ColumnFormulas.SortUnique(headerEnum.GetDescription(),
                                                                    stockSheet.GetRange(headerEnum.GetDescription(), 2));
                    break;
                
                case Header.WEEK_HIGH_52:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.WEEK_HIGH_52.GetDescription());
                    break;

                case Header.WEEK_LOW_52:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.WEEK_LOW_52.GetDescription());
                    break;

                default:
                    break;
            }
        });

        return sheet;
    }
}
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;

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
                Ticker = HeaderHelpers.GetStringValue(HeaderEnum.TICKER.GetDescription(), value, headers),
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Accounts = HeaderHelpers.GetIntValue(HeaderEnum.ACCOUNTS.GetDescription(), value, headers),
                Shares = HeaderHelpers.GetDecimalValue(HeaderEnum.SHARES.GetDescription(), value, headers),
                AverageCost = HeaderHelpers.GetDecimalValue(HeaderEnum.AVERAGE_COST.GetDescription(), value, headers),
                CostTotal = HeaderHelpers.GetDecimalValue(HeaderEnum.COST_TOTAL.GetDescription(), value, headers),
                CurrentPrice = HeaderHelpers.GetDecimalValue(HeaderEnum.CURRENT_PRICE.GetDescription(), value, headers),
                CurrentTotal = HeaderHelpers.GetDecimalValue(HeaderEnum.CURRENT_TOTAL.GetDescription(), value, headers),
                Return = HeaderHelpers.GetDecimalValue(HeaderEnum.RETURN.GetDescription(), value, headers),
                PeRatio = HeaderHelpers.GetDecimalValue(HeaderEnum.PE_RATIO.GetDescription(), value, headers),
                WeekHigh52 = HeaderHelpers.GetDecimalValue(HeaderEnum.WEEK_HIGH_52.GetDescription(), value, headers),
                WeekLow52 = HeaderHelpers.GetDecimalValue(HeaderEnum.WEEK_LOW_52.GetDescription(), value, headers),
                MaxHigh = HeaderHelpers.GetDecimalValue(HeaderEnum.MAX_HIGH.GetDescription(), value, headers),
                MinLow = HeaderHelpers.GetDecimalValue(HeaderEnum.MIN_LOW.GetDescription(), value, headers)
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
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.ACCOUNTS:
                    header.Formula = ColumnFormulas.CountIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(HeaderEnum.TICKER.GetDescription()),
                                                                    keyRange);
                    break;

                case HeaderEnum.AVERAGE_COST:
                    header.Note = ColumnNotes.AverageCost;
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.DivideRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(HeaderEnum.COST_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(HeaderEnum.SHARES.GetDescription()));
                    break;

                case HeaderEnum.COST_TOTAL:
                case HeaderEnum.CURRENT_TOTAL:
                case HeaderEnum.SHARES:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    stockSheet.GetRange(HeaderEnum.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    stockSheet.GetRange(headerEnum.GetDescription()));
                    break;

                case HeaderEnum.CURRENT_PRICE:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.PRICE.GetDescription());
                    break;

                case HeaderEnum.MAX_HIGH:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceMax(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.HIGH.GetDescription());
                    break;

                case HeaderEnum.MIN_LOW:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceMin(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.LOW.GetDescription());
                    break;

                case HeaderEnum.NAME:
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.NAME.GetDescription());
                    break;

                case HeaderEnum.PE_RATIO:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.PE_RATIO.GetDescription());
                    break;

                case HeaderEnum.RETURN:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(HeaderEnum.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(HeaderEnum.COST_TOTAL.GetDescription()));
                    break;

                case HeaderEnum.TICKER:
                    header.Formula = ColumnFormulas.SortUnique(headerEnum.GetDescription(),
                                                                    stockSheet.GetRange(headerEnum.GetDescription(), 2));
                    break;
                
                case HeaderEnum.WEEK_HIGH_52:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.WEEK_HIGH_52.GetDescription());
                    break;

                case HeaderEnum.WEEK_LOW_52:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    HeaderEnum.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributesEnum.WEEK_LOW_52.GetDescription());
                    break;

                default:
                    break;
            }
        });

        return sheet;
    }
}
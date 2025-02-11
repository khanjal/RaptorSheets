using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;

namespace RaptorSheets.Stock.Mappers;

public static class StockMapper
{
    public static List<StockEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var entities = new List<StockEntity>();
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

            StockEntity entity = new()
            {
                RowId = id,
                Account = HeaderHelpers.GetStringValue(HeaderEnum.ACCOUNT.GetDescription(), value, headers),
                Ticker = HeaderHelpers.GetStringValue(HeaderEnum.TICKER.GetDescription(), value, headers),
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
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
                MinLow = HeaderHelpers.GetDecimalValue(HeaderEnum.MIN_LOW.GetDescription(), value, headers),
            };

            entities.Add(entity);
        }
        return entities;
    }

    public static IList<IList<object?>> MapToRangeData(List<StockEntity> entities, IList<object> headers)
    {
        var rangeData = new List<IList<object?>>();

        foreach (var entity in entities)
        {
            var objectList = new List<object?>();

            foreach (var header in headers)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                // Console.WriteLine($"Header: {headerEnum}");

                switch (headerEnum)
                {
                    case HeaderEnum.ACCOUNT:
                        objectList.Add(entity.Account);
                        break;
                    case HeaderEnum.TICKER:
                        objectList.Add(entity.Ticker);
                        break;
                    case HeaderEnum.NAME:
                        objectList.Add(entity.Name);
                        break;
                    case HeaderEnum.SHARES:
                        objectList.Add(entity.Shares);
                        break;
                    case HeaderEnum.AVERAGE_COST:
                        objectList.Add(entity.AverageCost);
                        break;
                    case HeaderEnum.COST_TOTAL:
                        objectList.Add(entity.CostTotal);
                        break;
                    case HeaderEnum.CURRENT_PRICE:
                        objectList.Add(entity.CurrentPrice);
                        break;
                    case HeaderEnum.CURRENT_TOTAL:
                        objectList.Add(entity.CurrentTotal);
                        break;
                    case HeaderEnum.RETURN:
                        objectList.Add(entity.Return);
                        break;
                    case HeaderEnum.PE_RATIO:
                        objectList.Add(entity.PeRatio);
                        break;
                    case HeaderEnum.WEEK_HIGH_52:
                        objectList.Add(entity.WeekHigh52);
                        break;
                    case HeaderEnum.WEEK_LOW_52:
                        objectList.Add(entity.WeekLow52);
                        break;
                    case HeaderEnum.MAX_HIGH:
                        objectList.Add(entity.MaxHigh);
                        break;
                    case HeaderEnum.MIN_LOW:
                        objectList.Add(entity.MinLow);
                        break;
                    default:
                        objectList.Add(null);
                        break;
                }
            }

            // Console.WriteLine("Map Shift");
            // Console.WriteLine(JsonSerializer.Serialize(objectList));

            rangeData.Add(objectList);
        }

        return rangeData;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.StockSheet;
        sheet.Headers.UpdateColumns();

        var tickerSheet = SheetsConfig.TickerSheet;
        tickerSheet.Headers.UpdateColumns();

        var keyRange = GoogleConfig.KeyRange;
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.AVERAGE_COST:
                    header.Note = ColumnNotes.AverageCost;
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.COST_TOTAL:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.MultiplyRanges(headerEnum.GetDescription(), 
                                                                    keyRange, 
                                                                    sheet.GetLocalRange(HeaderEnum.SHARES.GetDescription()), 
                                                                    sheet.GetLocalRange(HeaderEnum.AVERAGE_COST.GetDescription()));
                    break;
                case HeaderEnum.CURRENT_PRICE:
                case HeaderEnum.MAX_HIGH:
                case HeaderEnum.MIN_LOW:
                case HeaderEnum.WEEK_HIGH_52:
                case HeaderEnum.WEEK_LOW_52:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(HeaderEnum.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(headerEnum.GetDescription()));
                    break;
                case HeaderEnum.CURRENT_TOTAL:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.MultiplyRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(HeaderEnum.SHARES.GetDescription()),
                                                                    sheet.GetLocalRange(HeaderEnum.CURRENT_PRICE.GetDescription()));
                    break;
                case HeaderEnum.PE_RATIO:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIfBlank(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(HeaderEnum.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(headerEnum.GetDescription()));
                    break;
                case HeaderEnum.RETURN:
                    header.Format = FormatEnum.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(HeaderEnum.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(HeaderEnum.COST_TOTAL.GetDescription()));
                    break;
                case HeaderEnum.SHARES:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;

                default:
                    break;
            }
        });

        return sheet;
    }
}
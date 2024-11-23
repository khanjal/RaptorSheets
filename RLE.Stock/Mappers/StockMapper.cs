using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Helpers;
using RLE.Core.Models.Google;
using RLE.Stock.Constants;
using RLE.Stock.Entities;
using RLE.Stock.Enums;

namespace RLE.Stock.Mappers;

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
                headers = HeaderHelper.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            };

            StockEntity entity = new()
            {
                Id = id,
                Account = HeaderHelper.GetStringValue(HeaderEnum.ACCOUNT.GetDescription(), value, headers),
                Ticker = HeaderHelper.GetStringValue(HeaderEnum.TICKER.GetDescription(), value, headers),
                Name = HeaderHelper.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Shares = HeaderHelper.GetDecimalValue(HeaderEnum.SHARES.GetDescription(), value, headers),
                AverageCost = HeaderHelper.GetDecimalValue(HeaderEnum.AVERAGE_COST.GetDescription(), value, headers),
                CostTotal = HeaderHelper.GetDecimalValue(HeaderEnum.COST_TOTAL.GetDescription(), value, headers),
                CurrentPrice = HeaderHelper.GetDecimalValue(HeaderEnum.CURRENT_PRICE.GetDescription(), value, headers),
                CurrentTotal = HeaderHelper.GetDecimalValue(HeaderEnum.CURRENT_TOTAL.GetDescription(), value, headers),
                Return = HeaderHelper.GetDecimalValue(HeaderEnum.RETURN.GetDescription(), value, headers),
                PeRatio = HeaderHelper.GetDecimalValue(HeaderEnum.PE_RATIO.GetDescription(), value, headers),
                WeekHigh52 = HeaderHelper.GetDecimalValue(HeaderEnum.WEEK_HIGH_52.GetDescription(), value, headers),
                WeekLow52 = HeaderHelper.GetDecimalValue(HeaderEnum.WEEK_LOW_52.GetDescription(), value, headers),
                MaxHigh = HeaderHelper.GetDecimalValue(HeaderEnum.MAX_HIGH.GetDescription(), value, headers),
                MinLow = HeaderHelper.GetDecimalValue(HeaderEnum.MIN_LOW.GetDescription(), value, headers),
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

        //var tripSheet = TripMapper.GetSheet();
        //var sheetTripsName = SheetEnum.TRIPS.GetDescription();
        //var sheetTripsTypeRange = tripSheet.Headers.First(x => x.Name == HeaderEnum.D.GetDescription()).Range;

        var range = sheet.GetLocalRange(HeaderEnum.ACCOUNT.GetDescription());
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
                    break;
                case HeaderEnum.CURRENT_PRICE:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.CURRENT_TOTAL:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.MAX_HIGH:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.MIN_LOW:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.PE_RATIO:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.RETURN:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.SHARES:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.WEEK_HIGH_52:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.WEEK_LOW_52:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}
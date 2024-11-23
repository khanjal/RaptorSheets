using RLE.Core.Extensions;
using RLE.Core.Helpers;
using RLE.Core.Models.Google;
using RLE.Stock.Constants;
using RLE.Stock.Entities;
using RLE.Stock.Enums;

namespace RLE.Stock.Mappers;

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
                headers = HeaderHelper.ParserHeader(value);
                continue;
            }

            if (value.Count < headers.Count)
            {
                value.AddItems(headers.Count - value.Count);
            };

            TickerEntity entity = new()
            {
                Id = id,
                Ticker = HeaderHelper.GetStringValue(HeaderEnum.TICKER.GetDescription(), value, headers),
                Name = HeaderHelper.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Accounts = HeaderHelper.GetIntValue(HeaderEnum.ACCOUNTS.GetDescription(), value, headers),
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

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.TickerSheet;

        //var stockSheet = TripMapper.GetSheet();

        // sheet.Headers = GigSheetHelpers.GetCommonTripGroupSheetHeaders(tripSheet, HeaderEnum.ADDRESS_END);

        return sheet;
    }
}
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using HeaderEnum = RaptorSheets.Stock.Enums.HeaderEnum;

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

    /// <summary>
    /// Maps StockEntity to Google Sheets RowData for ChangeSheetData/CreateUpdateCellRequests.
    /// Shares is the only genuinely user-editable column on the Stocks sheet - every other column
    /// (Account/Ticker/Name/AverageCost/CostTotal/CurrentPrice/CurrentTotal/Return/PeRatio/52-week
    /// high-low/MaxHigh/MinLow) is a cross-sheet formula or GOOGLEFINANCE pull, so writing to those
    /// would clobber the array formula. Non-Shares columns get an empty CellData placeholder to
    /// preserve column position without overwriting anything.
    /// </summary>
    public static IList<RowData> MapToRowData(List<StockEntity> entities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (var entity in entities)
        {
            var cells = new List<CellData>();

            foreach (var header in headers)
            {
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();

                cells.Add(headerEnum == HeaderEnum.SHARES
                    ? new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double)entity.Shares } }
                    : new CellData());
            }

            rows.Add(new RowData { Values = cells });
        }

        return rows;
    }

    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.StockSheet;
        var tickerSheet = SheetsConfig.TickerSheet;
        
        // Ensure column indexes are properly assigned
        sheet.Headers.UpdateColumns();
        
        // Apply header-specific configurations
        for (int i = 0; i < sheet.Headers.Count; i++)
        {
            var header = sheet.Headers[i];
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            var keyRange = GoogleConfig.KeyRange;

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
                    // Apply basic formatting based on header name patterns
                    ApplyBasicFormatting(header, header.Name);
                    break;
            }
        }
        
        return sheet;
    }

    /// <summary>
    /// Apply basic formatting patterns based on header content for Stock domain
    /// </summary>
    private static void ApplyBasicFormatting(SheetCellModel header, string headerName)
    {
        var lowerName = headerName.ToLowerInvariant();
        
        if (lowerName.Contains("cost") || lowerName.Contains("price") || lowerName.Contains("total") || 
            lowerName.Contains("return") || lowerName.Contains("high") || lowerName.Contains("low"))
            header.Format = FormatEnum.ACCOUNTING;
        else if (lowerName.Contains("ratio"))
            header.Format = FormatEnum.NUMBER;
        else
            header.Format = FormatEnum.TEXT;
    }
}
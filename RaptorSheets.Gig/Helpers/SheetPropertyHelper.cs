using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper class for parsing Google Sheets property data.
/// Handles the complex logic of extracting headers, row counts, and other metadata from Google Sheets API responses.
/// </summary>
public static class SheetPropertyHelper
{
    public static List<string> BuildCombinedRanges(List<string> sheets)
    {
        return sheets.SelectMany(sheet => new[]
        {
            $"{sheet}!{GoogleConfig.HeaderRange}",  // Headers (1:1)
            $"{sheet}!{GoogleConfig.RowRange}"      // First column data (A:A)
        }).ToList();
    }

    public static PropertyEntity ProcessSheetData(string sheetName, Spreadsheet? sheetInfo)
    {
        var property = new PropertyEntity
        {
            Name = sheetName,
            Id = "",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "" },
                { PropertyEnum.MAX_ROW.GetDescription(), "1000" },
                { PropertyEnum.MAX_ROW_VALUE.GetDescription(), "1" }
            }
        };

        var sheetData = sheetInfo?.Sheets?.FirstOrDefault(x => x.Properties.Title == sheetName);
        if (sheetData == null)
        {
            Console.WriteLine($"Warning: Sheet '{sheetName}' not found in API response");
            return property;
        }

        PopulateSheetBasicInfo(property, sheetData);
        ParseSheetDataRanges(property, sheetData);

        return property;
    }

    public static void PopulateSheetBasicInfo(PropertyEntity property, Sheet sheetData)
    {
        property.Id = sheetData.Properties.SheetId.ToString() ?? "";
        
        var maxRow = sheetData.Properties.GridProperties?.RowCount ?? 1000;
        property.Attributes[PropertyEnum.MAX_ROW.GetDescription()] = maxRow.ToString();
    }

    public static void ParseSheetDataRanges(PropertyEntity property, Sheet sheetData)
    {
        if (sheetData.Data == null || sheetData.Data.Count == 0)
        {
            Console.WriteLine($"Warning: No data returned for sheet '{property.Name}' ranges");
            return;
        }

        foreach (var dataRange in sheetData.Data)
        {
            if (dataRange?.RowData == null || dataRange.RowData.Count == 0) continue;

            var rangeType = DetermineRangeType(dataRange);
            ProcessDataRange(property, dataRange, rangeType);
        }
    }

    public static RangeType DetermineRangeType(GridData dataRange)
    {
        var firstRow = dataRange.RowData[0];
        var hasMultipleColumns = firstRow?.Values?.Count > 1;
        var hasMultipleRows = dataRange.RowData.Count > 1;

        return (hasMultipleRows, hasMultipleColumns) switch
        {
            (false, true) => RangeType.HeadersOnly,
            (true, false) => RangeType.ColumnDataOnly,
            (true, true) => RangeType.FullRange,
            _ => RangeType.Unknown
        };
    }

    public static void ProcessDataRange(PropertyEntity property, GridData dataRange, RangeType rangeType)
    {
        switch (rangeType)
        {
            case RangeType.HeadersOnly:
                ProcessHeadersRange(property, dataRange);
                break;
                
            case RangeType.ColumnDataOnly:
                ProcessColumnDataRange(property, dataRange);
                break;
                
            case RangeType.FullRange:
                ProcessFullRange(property, dataRange);
                break;
                
            case RangeType.Unknown:
                // Skip unknown range types
                break;
        }
    }

    public static void ProcessHeadersRange(PropertyEntity property, GridData dataRange)
    {
        var firstRow = dataRange.RowData[0];
        if (firstRow?.Values == null) return;

        var headers = string.Join(",", firstRow.Values
            .Where(x => x.FormattedValue != null)
            .Select(x => x.FormattedValue));
            
        property.Attributes[PropertyEnum.HEADERS.GetDescription()] = headers;
    }

    public static void ProcessColumnDataRange(PropertyEntity property, GridData dataRange)
    {
        var maxRowValue = FindLastRowWithData(dataRange);
        property.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()] = maxRowValue.ToString();
    }

    public static void ProcessFullRange(PropertyEntity property, GridData dataRange)
    {
        // Extract headers from first row if available
        var firstRow = dataRange.RowData[0];
        if (firstRow?.Values?.Count > 1)
        {
            ProcessHeadersRange(property, dataRange);
        }

        // Find last row with data in first column
        var maxRowValue = FindLastRowWithData(dataRange);
        property.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()] = maxRowValue.ToString();
    }

    public static int FindLastRowWithData(GridData dataRange)
    {
        for (int i = dataRange.RowData.Count - 1; i > 0; i--)
        {
            var cell = dataRange.RowData[i]?.Values?.FirstOrDefault();
            if (cell != null && !string.IsNullOrEmpty(cell.FormattedValue))
            {
                return i + 1; // +1 because row index is zero-based
            }
        }
        return 1; // Default to header row
    }
}

/// <summary>
/// Enumeration for different types of data ranges in Google Sheets API responses.
/// </summary>
public enum RangeType
{
    Unknown,
    HeadersOnly,      // Single row, multiple columns (1:1 range)
    ColumnDataOnly,   // Multiple rows, single column (A:A range)
    FullRange        // Multiple rows and columns (full sheet)
}
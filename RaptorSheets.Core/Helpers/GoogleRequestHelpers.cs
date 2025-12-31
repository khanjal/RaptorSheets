using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

public static class GoogleRequestHelpers
{

    public static Request GenerateAppendCells(SheetModel sheet)
    {
        // Create Sheet Headers
        var appendCellsRequest = new AppendCellsRequest
        {
            Fields = FieldEnum.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(),
            Rows = SheetHelpers.HeadersToRowData(sheet!),
            SheetId = sheet!.Id
        };

        return new Request { AppendCells = appendCellsRequest };
    }

    public static Request GenerateAppendCells(int sheetId, IList<RowData> rows)
    {
        // Create Sheet Data
        var appendCellsRequest = new AppendCellsRequest
        {
            Fields = FieldEnum.USER_ENTERED_VALUE.GetDescription(),
            Rows = rows,
            SheetId = sheetId
        };
        return new Request { AppendCells = appendCellsRequest };
    }

    public static Request? GenerateAppendDimension(SheetModel sheet)
    {
        // Append more columns if the default amount isn't enough
        var defaultColumns = GoogleConfig.DefaultColumnCount;

        if (sheet!.Headers.Count <= defaultColumns)
        {
            return null;
        }

        var appendDimensionRequest = new AppendDimensionRequest
        {
            Dimension = DimensionEnum.COLUMNS.GetDescription(),
            Length = sheet.Headers.Count - defaultColumns,
            SheetId = sheet.Id
        };

        var request = new Request { AppendDimension = appendDimensionRequest };

        return request;
    }

    public static Request GenerateAppendDimension(int sheetId, int rows)
    {
        var appendDimensionRequest = new AppendDimensionRequest
        {
            Dimension = DimensionEnum.ROWS.GetDescription(),
            Length = rows,
            SheetId = sheetId
        };
        var request = new Request { AppendDimension = appendDimensionRequest };

        return request;
    }

    public static Request GenerateBandingRequest(SheetModel sheet)
    {
        // Add alternating colors
        var addBandingRequest = new AddBandingRequest
        {
            BandedRange = new BandedRange
            {
                BandedRangeId = sheet!.Id,
                Range = new GridRange { SheetId = sheet.Id },
                RowProperties = new BandingProperties { HeaderColor = SheetHelpers.GetColor(sheet!.TabColor), FirstBandColor = SheetHelpers.GetColor(ColorEnum.WHITE), SecondBandColor = SheetHelpers.GetColor(sheet!.CellColor) }
            }
        };
        return new Request { AddBanding = addBandingRequest };
    }

    public static List<Request> GenerateDeleteSheetRequests(List<PropertyEntity> sheetProperties)
    {
        var requests = new List<Request>();
        
        if (sheetProperties?.Count == 0 || sheetProperties == null)
        {
            return requests;
        }

        // Create delete sheet requests for sheets that exist (have valid IDs)
        var sheetIdsToDelete = sheetProperties
            .Where(p => !string.IsNullOrEmpty(p.Id) && int.TryParse(p.Id, out _))
            .Select(p => int.Parse(p.Id))
            .ToList();

        foreach (var sheetId in sheetIdsToDelete)
        {
            requests.Add(new Request
            {
                DeleteSheet = new DeleteSheetRequest
                {
                    SheetId = sheetId
                }
            });
        }

        return requests;
    }

    public static List<Tuple<int, int>> GenerateIndexRanges(List<int> rowIds)
    {
        // Convert rowIds to index ranges
        var indexRanges = new List<Tuple<int, int>>();

        if (rowIds.Count == 0)
        {
            return indexRanges;
        }

        // Sort rowIds in descending order to delete from bottom to top
        // This prevents row shifting issues when deleting multiple rows
        var sortedRowIds = rowIds.OrderByDescending(x => x).ToList();

        var startIndex = sortedRowIds[0] - 1; // Initialize with first row ID (convert to 0-based index)
        var endIndex = sortedRowIds[0]; // End index is exclusive, so this is the row after the one to delete

        foreach (var rowId in sortedRowIds.Skip(1)) // Skip the first element since we already processed it
        {
            // If the rowId is consecutive (going backwards) extend the range
            if (rowId == startIndex) // Previous row (going backwards)
            {
                startIndex = rowId - 1; // Extend range downward
            }
            else // Start a new index range
            {
                indexRanges.Add(new Tuple<int, int>(startIndex, endIndex));
                startIndex = rowId - 1; // Convert to 0-based index
                endIndex = rowId; // End index is exclusive
            }
        }

        // Add the last index range
        indexRanges.Add(new Tuple<int, int>(startIndex, endIndex));
        return indexRanges;
    }

    public static List<Request> GenerateDeleteRequests(int sheetId, List<int> rowIds)
    {
        if (rowIds.Count == 0)
        {
            return new List<Request>();
        }

        // Sort rowIds in descending order to delete from bottom to top
        // This prevents row shifting issues when deleting multiple rows
        var sortedRowIds = rowIds.OrderByDescending(x => x).ToList();
        
        var requests = new List<Request>();

        // Create individual delete requests for each row
        // Google Sheets API handles the deletion more reliably when done one by one
        foreach (var rowId in sortedRowIds)
        {
            var deleteDimension = new DeleteDimensionRequest
            {
                Range = new DimensionRange
                {
                    Dimension = DimensionEnum.ROWS.GetDescription(),
                    SheetId = sheetId,
                    StartIndex = rowId - 1, // Convert to 0-based index
                    EndIndex = rowId // Exclusive end, so this deletes exactly one row
                }
            };

            requests.Add(new Request { DeleteDimension = deleteDimension });
        }

        return requests;
    }

    public static List<Request> GenerateDeleteRequests(int sheetId, List<Tuple<int, int>> indexRanges)
    {
        var requests = new List<Request>();

        foreach (var indexRange in indexRanges)
        {
            var deleteDimension = new DeleteDimensionRequest
            {
                Range = new DimensionRange
                {
                    Dimension = DimensionEnum.ROWS.GetDescription(),
                    SheetId = sheetId,
                    StartIndex = indexRange.Item1,
                    EndIndex = indexRange.Item2
                }
            };

            requests.Add(new Request { DeleteDimension = deleteDimension });
        }

        return requests;
    }

    public static Request GenerateInsertDimension(int sheetId, int startIndex, int endIndex)
    {
        var insertRequest = new Request
        {
            InsertDimension = new InsertDimensionRequest
            {
                Range = new DimensionRange
                {
                    SheetId = sheetId,
                    Dimension = "ROWS",
                    StartIndex = startIndex,
                    EndIndex = endIndex
                },
                InheritFromBefore = true
            }
        };

        return insertRequest;
    }

    public static BatchGetValuesByDataFilterRequest GenerateBatchGetValuesByDataFilterRequest(List<string> sheets, string? range = "")
    {
        if (sheets == null || sheets.Count < 1)
        {
            return new BatchGetValuesByDataFilterRequest();
        }

        var request = new BatchGetValuesByDataFilterRequest
        {
            DataFilters = []
        };
        foreach (var sheet in sheets)
        {
            var filter = new DataFilter
            {
                A1Range = !string.IsNullOrWhiteSpace(range) ? $"{sheet}!{range}" : sheet
            };
            request.DataFilters.Add(filter);
        }
        return request;
    }

    public static Request GenerateProtectedRangeForHeaderOrSheet(SheetModel sheet)
    {
        // Protect sheet or header
        var addProtectedRangeRequest = new AddProtectedRangeRequest();
        if (sheet!.ProtectSheet)
        {
            addProtectedRangeRequest = new AddProtectedRangeRequest
            {
                ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.SheetWarning, Range = new GridRange { SheetId = sheet.Id }, WarningOnly = true }
            };
        }
        else
        {
            // Protect full header if sheet isn't protected.
            var range = new GridRange
            {
                SheetId = sheet.Id,
                StartColumnIndex = 0,
                EndColumnIndex = sheet!.Headers.Count,
                StartRowIndex = 0,
                EndRowIndex = 1
            };

            addProtectedRangeRequest.ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.HeaderWarning, Range = range, WarningOnly = true };
        }

        return new Request { AddProtectedRange = addProtectedRangeRequest };
    }

    public static Request GenerateColumnProtection(GridRange range)
    {
        var addProtectedRangeRequest = new AddProtectedRangeRequest
        {
            ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.ColumnWarning, Range = range, WarningOnly = true }
        };

        return new Request { AddProtectedRange = addProtectedRangeRequest };
    }

    public static RepeatCellRequest GenerateRepeatCellRequest(RepeatCellModel repeatCellModel)
    {
        // Set start/end for formatting
        repeatCellModel.GridRange.StartRowIndex = 1;
        repeatCellModel.GridRange.EndRowIndex = null;

        var repeatCellRequest = new RepeatCellRequest
        {
            Fields = FieldEnum.USER_ENTERED_VALUE_AND_FORMAT.GetDescription(),
            Range = repeatCellModel.GridRange,
            Cell = new CellData()
        };

        if (repeatCellModel.CellFormat != null)
        {
            repeatCellRequest.Cell.UserEnteredFormat = repeatCellModel.CellFormat;
        }

        if (repeatCellModel.DataValidation != null)
        {
            repeatCellRequest.Cell.DataValidation = repeatCellModel.DataValidation;
        }

        return repeatCellRequest;
    }

    public static Request GenerateSheetPropertes(SheetModel sheet)
    {
        var sheetRequest = new AddSheetRequest
        {
            Properties = new SheetProperties
            {
                // Create Sheet With Properties
                SheetId = sheet.Id,
                Title = sheet!.Name,
                TabColor = SheetHelpers.GetColor(sheet.TabColor),
                GridProperties = new GridProperties { FrozenColumnCount = sheet.FreezeColumnCount, FrozenRowCount = sheet.FreezeRowCount }
            }
        };

        return new Request { AddSheet = sheetRequest };
    }

    public static Request GenerateUpdateCellsRequest(int sheetId, int rowIndex, IList<RowData> rows) 
    {
        // Indexes are 1 less than rowIds
        var range = new GridRange
        {
            SheetId = sheetId,
            StartRowIndex = rowIndex,
            EndRowIndex = rowIndex + 1,
        };

        // Create Sheet Data
        var updateCellsRequest = new UpdateCellsRequest
        {
            Fields = FieldEnum.USER_ENTERED_VALUE.GetDescription(),
            Rows = rows,
            Range = range,
        };
        return new Request { UpdateCells = updateCellsRequest };
    }

    public static BatchUpdateValuesRequest GenerateUpdateValueRequest(string sheetName, IDictionary<int, IList<IList<object?>>> rowValues)
    {
        var valueRanges = new List<ValueRange>();

        foreach (var rowValue in rowValues)
        {
            var valueRange = new ValueRange
            {
                MajorDimension = "ROWS",
                Range = $"{sheetName}!A{rowValue.Key}",
                Values = rowValue.Value
            };
            valueRanges.Add(valueRange);
        }

        var batchUpdateValuesRequest = new BatchUpdateValuesRequest
        {
            Data = valueRanges,
            ValueInputOption = ValueInputOptionEnum.USER_ENTERED.GetDescription()
        };

        return batchUpdateValuesRequest;
    }

    /// <summary>
    /// Generates a request to update a sheet's tab index (position).
    /// </summary>
    /// <param name="sheetId">The ID of the sheet to move.</param>
    /// <param name="index">The target zero-based index position for the sheet tab.</param>
    /// <returns>A <see cref="Request"/> containing the UpdateSheetProperties request.</returns>
    public static Request GenerateUpdateSheetIndex(int sheetId, int index)
    {
        var updateRequest = new UpdateSheetPropertiesRequest
        {
            Properties = new SheetProperties
            {
                SheetId = sheetId,
                Index = index
            },
            Fields = "index"
        };

        return new Request { UpdateSheetProperties = updateRequest };
    }

    /// <summary>
    /// Computes the target zero-based sheet index for moving a sheet to the end position
    /// after adding new sheets in the same batch operation.
    /// </summary>
    /// <param name="existingSheetCount">
    /// The number of sheets that exist in the spreadsheet before the batch operation.
    /// </param>
    /// <param name="newSheetCount">
    /// The number of new sheets being added in the same batch operation.
    /// </param>
    /// <returns>
    /// The target index at which a sheet can be placed to position it after all
    /// existing and newly added sheets. This is calculated as 
    /// <c>existingSheetCount + newSheetCount</c>.
    /// </returns>
    public static int ComputeEndIndex(int existingSheetCount, int newSheetCount)
    {
        return existingSheetCount + newSheetCount;
    }

    #region Formatting Requests

    /// <summary>
    /// Generates a request to update the number format for a cell range.
    /// </summary>
    /// <param name="sheetId">The ID of the sheet containing the range.</param>
    /// <param name="startRowIndex">The zero-based start row index (inclusive).</param>
    /// <param name="endRowIndex">The zero-based end row index (exclusive).</param>
    /// <param name="startColumnIndex">The zero-based start column index (inclusive).</param>
    /// <param name="endColumnIndex">The zero-based end column index (exclusive).</param>
    /// <param name="numberFormatType">The number format type (e.g., "TEXT", "CURRENCY", "DATE").</param>
    /// <param name="numberFormatPattern">The number format pattern (e.g., "$#,##0.00", "yyyy-mm-dd").</param>
    /// <returns>A Request containing the format update.</returns>
    public static Request GenerateUpdateNumberFormat(
        int sheetId,
        int startRowIndex,
        int endRowIndex,
        int startColumnIndex,
        int endColumnIndex,
        string numberFormatType,
        string? numberFormatPattern = null)
    {
        var updateCellsRequest = new UpdateCellsRequest
        {
            Range = new GridRange
            {
                SheetId = sheetId,
                StartRowIndex = startRowIndex,
                EndRowIndex = endRowIndex,
                StartColumnIndex = startColumnIndex,
                EndColumnIndex = endColumnIndex
            },
            Fields = "userEnteredFormat.numberFormat"
        };

        // Create a single row of cell data for the format
        var rows = new List<RowData>();
        for (int i = startRowIndex; i < endRowIndex; i++)
        {
            var cellData = new List<CellData>();
            for (int j = startColumnIndex; j < endColumnIndex; j++)
            {
                cellData.Add(new CellData
                {
                    UserEnteredFormat = new CellFormat
                    {
                        NumberFormat = new NumberFormat
                        {
                            Type = numberFormatType,
                            Pattern = numberFormatPattern
                        }
                    }
                });
            }
            rows.Add(new RowData { Values = cellData });
        }

        updateCellsRequest.Rows = rows;

        return new Request { UpdateCells = updateCellsRequest };
    }

    /// <summary>
    /// Generates a request to update cell background colors.
    /// </summary>
    /// <param name="sheetId">The ID of the sheet containing the range.</param>
    /// <param name="startRowIndex">The zero-based start row index (inclusive).</param>
    /// <param name="endRowIndex">The zero-based end row index (exclusive).</param>
    /// <param name="startColumnIndex">The zero-based start column index (inclusive).</param>
    /// <param name="endColumnIndex">The zero-based end column index (exclusive).</param>
    /// <param name="red">The red component (0-1).</param>
    /// <param name="green">The green component (0-1).</param>
    /// <param name="blue">The blue component (0-1).</param>
    /// <param name="alpha">The alpha component (0-1, defaults to 1).</param>
    /// <returns>A Request containing the color update.</returns>
    public static Request GenerateUpdateCellColor(
        int sheetId,
        int startRowIndex,
        int endRowIndex,
        int startColumnIndex,
        int endColumnIndex,
        float red,
        float green,
        float blue,
        float alpha = 1f)
    {
        var updateCellsRequest = new UpdateCellsRequest
        {
            Range = new GridRange
            {
                SheetId = sheetId,
                StartRowIndex = startRowIndex,
                EndRowIndex = endRowIndex,
                StartColumnIndex = startColumnIndex,
                EndColumnIndex = endColumnIndex
            },
            Fields = "userEnteredFormat.backgroundColor"
        };

        var rows = new List<RowData>();
        for (int i = startRowIndex; i < endRowIndex; i++)
        {
            var cellData = new List<CellData>();
            for (int j = startColumnIndex; j < endColumnIndex; j++)
            {
                cellData.Add(new CellData
                {
                    UserEnteredFormat = new CellFormat
                    {
                        BackgroundColor = new Color
                        {
                            Red = red,
                            Green = green,
                            Blue = blue,
                            Alpha = alpha
                        }
                    }
                });
            }
            rows.Add(new RowData { Values = cellData });
        }

        updateCellsRequest.Rows = rows;

        return new Request { UpdateCells = updateCellsRequest };
    }

    /// <summary>
    /// Generates a request to update sheet tab color.
    /// </summary>
    /// <param name="sheetId">The ID of the sheet.</param>
    /// <param name="red">The red component (0-1).</param>
    /// <param name="green">The green component (0-1).</param>
    /// <param name="blue">The blue component (0-1).</param>
    /// <param name="alpha">The alpha component (0-1, defaults to 1).</param>
    /// <returns>A Request containing the tab color update.</returns>
    public static Request GenerateUpdateTabColor(
        int sheetId,
        float red,
        float green,
        float blue,
        float alpha = 1f)
    {
        var updateSheetPropertiesRequest = new UpdateSheetPropertiesRequest
        {
            Properties = new SheetProperties
            {
                SheetId = sheetId,
                TabColor = new Color
                {
                    Red = red,
                    Green = green,
                    Blue = blue,
                    Alpha = alpha
                }
            },
            Fields = "tabColor"
        };

        return new Request { UpdateSheetProperties = updateSheetPropertiesRequest };
    }

    /// <summary>
    /// Generates a request to update frozen rows in a sheet.
    /// </summary>
    /// <param name="sheetId">The ID of the sheet.</param>
    /// <param name="frozenRowCount">The number of rows to freeze (0 to unfreeze).</param>
    /// <param name="frozenColumnCount">The number of columns to freeze (0 to unfreeze).</param>
    /// <returns>A Request containing the frozen row/column update.</returns>
    public static Request GenerateUpdateFrozenRowsColumns(
        int sheetId,
        int frozenRowCount,
        int frozenColumnCount)
    {
        var updateSheetPropertiesRequest = new UpdateSheetPropertiesRequest
        {
            Properties = new SheetProperties
            {
                SheetId = sheetId,
                GridProperties = new GridProperties
                {
                    FrozenRowCount = frozenRowCount,
                    FrozenColumnCount = frozenColumnCount
                }
            },
            Fields = "gridProperties.frozenRowCount,gridProperties.frozenColumnCount"
        };

        return new Request { UpdateSheetProperties = updateSheetPropertiesRequest };
    }

    /// <summary>
    /// Generates a request to protect a sheet.
    /// </summary>
    /// <param name="sheetId">The ID of the sheet to protect.</param>
    /// <param name="title">The title/description of the protected sheet.</param>
    /// <returns>A Request containing the sheet protection.</returns>
    public static Request GenerateProtectSheet(int sheetId, string? title = null)
    {
        var addProtectedRangeRequest = new AddProtectedRangeRequest
        {
            ProtectedRange = new ProtectedRange
            {
                Range = new GridRange { SheetId = sheetId },
                Description = title ?? $"Protected sheet {sheetId}",
                WarningOnly = true
            }
        };

        return new Request { AddProtectedRange = addProtectedRangeRequest };
    }

    #endregion
}

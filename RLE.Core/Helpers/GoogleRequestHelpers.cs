using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using System;

namespace RaptorSheets.Core.Helpers;

public static class GoogleRequestHelpers
{

    public static Request GenerateAppendCells(SheetModel sheet)
    {
        // Create Sheet Headers
        var appendCellsRequest = new AppendCellsRequest
        {
            Fields = GoogleConfig.FieldsUpdate,
            Rows = SheetHelpers.HeadersToRowData(sheet!),
            SheetId = sheet!.Id
        };

        return new Request { AppendCells = appendCellsRequest };
    }

    public static List<Request> GenerateAppendDimension(SheetModel sheet)
    {
        List<Request> requests = [];
        // Append more columns if the default amount isn't enough
        var defaultColumns = GoogleConfig.DefaultColumnCount;
        if (sheet!.Headers.Count > defaultColumns)
        {
            var appendDimensionRequest = new AppendDimensionRequest
            {
                Dimension = GoogleConfig.AppendDimensionType,
                Length = sheet.Headers.Count - defaultColumns,
                SheetId = sheet.Id
            };
            requests.Add(new Request { AppendDimension = appendDimensionRequest });
        }

        return requests;
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
            Fields = GoogleConfig.FieldsUpdate,
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
}

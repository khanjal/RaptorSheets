using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Utilities.Google
{
    public static class GenerateSheets
    {
        public static BatchUpdateSpreadsheetRequest Generate(List<SheetModel> sheets)
        {
            // var sheets = SheetHelper.GetSheets();

            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = [];
            var repeatCellRequests = new List<RepeatCellRequest>();
            var protectCellRequests = new List<AddProtectedRangeRequest>();

            sheets.ForEach(sheet => {
                var random = new Random();
                // var sheetId = (int?)(DateTimeOffset.Now.ToUnixTimeMilliseconds() % 1000000000);
                var sheetId = random.Next();

                var sheetRequest = new AddSheetRequest();
                sheetRequest.Properties = new SheetProperties();

                // TODO: Make request helper to build these requests.

                // Create Sheet With Properties
                sheetRequest.Properties.SheetId = sheetId;
                // sheetRequest.Properties.Title = $"{sheet.Name} {sheetId}";
                sheetRequest.Properties.Title = sheet.Name;
                sheetRequest.Properties.TabColor = SheetHelper.GetColor(sheet.TabColor);
                sheetRequest.Properties.GridProperties = new GridProperties { FrozenColumnCount = sheet.FreezeColumnCount, FrozenRowCount = sheet.FreezeRowCount };

                batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddSheet = sheetRequest });

                // Append more columns if the default amount isn't enough
                var defaultColumns = 26;
                if (sheet.Headers.Count > defaultColumns)
                {
                    var appendDimensionRequest = new AppendDimensionRequest();
                    appendDimensionRequest.Dimension = "COLUMNS";
                    appendDimensionRequest.Length = sheet.Headers.Count - defaultColumns;
                    appendDimensionRequest.SheetId = sheetId;
                    batchUpdateSpreadsheetRequest.Requests.Add(new Request { AppendDimension = appendDimensionRequest });
                }

                // Create Sheet Headers
                var appendCellsRequest = new AppendCellsRequest();
                appendCellsRequest.Fields = "*";
                appendCellsRequest.Rows = SheetHelper.HeadersToRowData(sheet);
                appendCellsRequest.SheetId = sheetId;

                batchUpdateSpreadsheetRequest.Requests.Add(new Request { AppendCells = appendCellsRequest });

                // Format/Protect Column Cells
                sheet.Headers.ForEach(header => {
                    var range = new GridRange
                    {
                        SheetId = sheetId,
                        StartColumnIndex = header.Index,
                        EndColumnIndex = header.Index + 1,
                        StartRowIndex = 1,
                    };

                    // If whole sheet isn't protected then protect certain columns
                    if (!string.IsNullOrEmpty(header.Formula) && !sheet.ProtectSheet)
                    {
                        var addProtectedRangeRequest = new AddProtectedRangeRequest
                        {
                            ProtectedRange = new ProtectedRange { Description = "Editing this column will cause a #REF error.", Range = range, WarningOnly = true }
                        };
                        // protectCellRequests.Add(addProtectedRangeRequest);
                        batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
                    }

                    // If there's no format or validation then go to next header
                    if (header.Format == null && header.Validation == null)
                    {
                        return;
                    }

                    // Set start/end for formatting
                    range.StartRowIndex = 1;
                    range.EndRowIndex = null;

                    var repeatCellRequest = new RepeatCellRequest();
                    repeatCellRequest.Fields = "*";
                    repeatCellRequest.Range = range;
                    repeatCellRequest.Cell = new CellData();

                    if (header.Format != null)
                    {
                        repeatCellRequest.Cell.UserEnteredFormat = SheetHelper.GetCellFormat((FormatEnum)header.Format);
                    }

                    if (header.Validation != null)
                    {
                        repeatCellRequest.Cell.DataValidation = SheetHelper.GetDataValidation((ValidationEnum)header.Validation);
                    }

                    repeatCellRequests.Add(repeatCellRequest);
                    //batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = repeatCellRequest });
                });

                // Add alternating colors
                var addBandingRequest = new AddBandingRequest();
                addBandingRequest.BandedRange = new BandedRange
                {
                    BandedRangeId = sheetId,
                    Range = new GridRange { SheetId = sheetId },
                    RowProperties = new BandingProperties { HeaderColor = SheetHelper.GetColor(sheet.TabColor), FirstBandColor = SheetHelper.GetColor(ColorEnum.WHITE), SecondBandColor = SheetHelper.GetColor(sheet.CellColor) }
                };
                batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddBanding = addBandingRequest });

                // Protect sheet or header
                var addProtectedRangeRequest = new AddProtectedRangeRequest();
                if (sheet.ProtectSheet)
                {
                    addProtectedRangeRequest = new AddProtectedRangeRequest
                    {
                        ProtectedRange = new ProtectedRange { Description = "Editing this sheet will cause a #REF error.", Range = new GridRange { SheetId = sheetId }, WarningOnly = true }
                    };
                    batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
                }
                else
                {
                    // Protect full header if sheet isn't protected.
                    var range = new GridRange
                    {
                        SheetId = sheetId,
                        StartColumnIndex = 0,
                        EndColumnIndex = sheet.Headers.Count,
                        StartRowIndex = 0,
                        EndRowIndex = 1
                    };

                    addProtectedRangeRequest.ProtectedRange = new ProtectedRange { Description = "Editing the header could cause a #REF error or break sheet references.", Range = range, WarningOnly = true };
                    batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
                }

                // _sheet.Messages.Add($"Sheet [{sheet.Name}]: Added");
            });

            repeatCellRequests.ForEach(request => {
                batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
            });

            protectCellRequests.ForEach(request => {
                batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddProtectedRange = request });
            });

            return batchUpdateSpreadsheetRequest;
            // Console.WriteLine(JsonSerializer.Serialize(batchUpdateSpreadsheetRequest.Requests));
            //var batchUpdateRequest = _googleSheetService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);
            //batchUpdateRequest.Execute();
        }
    }
}
